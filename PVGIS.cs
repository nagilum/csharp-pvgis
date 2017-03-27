using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace PVGIS {
    public class PvgisWrapper {
        /// <summary>
        /// Get PVGIS values for the given lat/lng.
        /// </summary>
        public static PvgisResponse Get(decimal lat, decimal lng) {
            const string url = "http://re.jrc.ec.europa.eu/pvgis/apps4/PVcalc.php";

            var slat = lat.ToString(new CultureInfo("en-US"));
            var slng = lng.ToString(new CultureInfo("en-US"));

            var dict = new Dictionary<string, string> {
                {"MAX_FILE_SIZE", "10000"},
                {"pv_database", "PVGIS-classic"},
                {"pvtechchoice", "crystSi"},
                {"peakpower", "1"},
                {"efficiency", "14"},
                {"mountingplace", "free"},
                {"angle", "35"},
                {"aspectangle", "0"},
                {"horizonfile", ""},
                {"outputchoicebuttons", "window"},
                {"sbutton", "Calculate"},
                {"outputformatchoice", "window"},
                {"optimalchoice", ""},
                {"latitude", slat},
                {"longitude", slng},
                {"regionname", "europe"},
                {"language", "en_en"}
            };

            var queryString = dict.Aggregate("", (c, p) => c + ("&" + p.Key + "=" + HttpUtility.UrlEncode(p.Value))).Substring(1);
            var bytes = Encoding.UTF8.GetBytes(queryString);

            var req = WebRequest.Create(url) as HttpWebRequest;

            if (req == null) {
                throw new Exception("Could not establish connection to PVGIS database.");
            }

            req.Method = "POST";
            req.Expect = null;
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = bytes.Length;

            var stream = req.GetRequestStream();

            stream.Write(bytes, 0, bytes.Length);
            stream.Close();

            var res = req.GetResponse() as HttpWebResponse;

            if (res == null) {
                throw new Exception("Could not receive data from PVGIS database.");
            }

            stream = res.GetResponseStream();

            if (stream == null) {
                throw new Exception("Datastream is invalid.");
            }

            var reader = new StreamReader(stream);
            var html = reader.ReadToEnd();

            reader.Close();
            stream.Close();

            if (html.IndexOf("no valid daily radiation data", StringComparison.InvariantCultureIgnoreCase) > -1) {
                throw new Exception("No valid daily radiation data for location for today.");
            }

            return parseHTML(html);
        }

        /// <summary>
        /// Parse the HTML response from the PVGIS database.
        /// </summary>
        private static PvgisResponse parseHTML(string html) {
            return new PvgisResponse {
                NominalPower = parseDecimal(getValue(ref html, "Nominal power of the PV system")),
                EstTempLowIrrLoss = parseDecimal(getValue(ref html, "Estimated losses due to temperature and low irradiance")),
                EstAngReflLoss = parseDecimal(getValue(ref html, "Estimated loss due to angular reflectance effects")),
                OtherLosses = parseDecimal(getValue(ref html, "Other losses (cables, inverter etc.)")),
                CombinedLosses = parseDecimal(getValue(ref html, "Combined PV system losses")),
                MonthlyAverage = new Dictionary<string, PvgisResponseMonth> {
                    { "Jan", get4ColumnRow(ref html, "Jan") },
                    { "Feb", get4ColumnRow(ref html, "Feb") },
                    { "Mar", get4ColumnRow(ref html, "Mar") },
                    { "Apr", get4ColumnRow(ref html, "Apr") },
                    { "May", get4ColumnRow(ref html, "May") },
                    { "Jun", get4ColumnRow(ref html, "Jun") },
                    { "Jul", get4ColumnRow(ref html, "Jul") },
                    { "Aug", get4ColumnRow(ref html, "Aug") },
                    { "Sep", get4ColumnRow(ref html, "Sep") },
                    { "Oct", get4ColumnRow(ref html, "Oct") },
                    { "Nov", get4ColumnRow(ref html, "Nov") },
                    { "Dec", get4ColumnRow(ref html, "Dec") },
                },
                YearlyAverage = get4ColumnRow(ref html, "Yearly average"),
                YearlyTotal = get2ColumnRow(ref html, "Total for year")
            };
        }

        /// <summary>
        /// Convert given value to decimal.
        /// </summary>
        private static decimal parseDecimal(string value) {
            if (value == null) {
                return 0;
            }

            decimal d;

            return decimal.TryParse(value.Trim(), NumberStyles.Any, new CultureInfo("en-US"), out d)
                ? d
                : 0;
        }

        /// <summary>
        /// Parse HTML and get a single line value.
        /// </summary>
        private static string getValue(ref string html, string key) {
            var sp = html.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);

            if (sp == -1) {
                return null;
            }

            sp = html.IndexOf(": ", sp, StringComparison.InvariantCultureIgnoreCase);

            var temp = html.Substring(sp + 2);
            var output = string.Empty;

            for (var i = 0; i < temp.Length; i++) {
                var chr = temp.Substring(i, 1);

                if (chr == "0" || chr == "1" || chr == "2" || chr == "3" ||
                    chr == "4" || chr == "5" || chr == "6" || chr == "7" ||
                    chr == "8" || chr == "9" || chr == ".") {
                    output += chr;
                }
                else {
                    break;
                }
            }

            return output;
        }

        /// <summary>
        /// Parse HTML and get 4 values from a table row.
        /// </summary>
        private static PvgisResponseMonth get4ColumnRow(ref string html, string monthName) {
            var key = "<td> " + monthName + " </td>";
            var sp = html.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);
            
            if (sp == -1) {
                key = "<td><b> " + monthName + " </b></td>";
                sp = html.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);
            }

            if (sp == -1) {
                return null;
            }

            var temp = html.Substring(sp + key.Length);

            sp = temp.IndexOf("</td></tr>", StringComparison.InvariantCultureIgnoreCase);

            if (sp == -1) {
                return null;
            }

            temp = temp.Substring(0, sp)
                .Replace("</td>", "")
                .Replace("<b>", "")
                .Replace("</b>", "")
                .Replace("<td align=\"right\">", ",");

            var values = temp.Split(',');

            if (values.Length != 5) {
                return null;
            }

            return new PvgisResponseMonth {
                Ed = parseDecimal(values[1]),
                Em = parseDecimal(values[2]),
                Hd = parseDecimal(values[3]),
                Hm = parseDecimal(values[4])
            };
        }

        /// <summary>
        /// Parse HTML and get 2 values from a table row.
        /// </summary>
        private static PvgisResponseYear get2ColumnRow(ref string html, string title) {
            var key = "<td><b>" + title + "</b></td>";
            var sp = html.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);

            if (sp == -1) {
                return null;
            }

            var temp = html.Substring(sp + key.Length);

            sp = temp.IndexOf("</td> </tr>", StringComparison.InvariantCultureIgnoreCase);

            if (sp == -1) {
                return null;
            }

            temp = temp.Substring(0, sp)
                .Replace("<td align=\"right\" colspan=2 >", ",")
                .Replace("<b>", "")
                .Replace("</b>", "")
                .Replace("</td>", "");

            var values = temp.Split(',');

            if (values.Length != 3) {
                return null;
            }

            return new PvgisResponseYear {
                E = parseDecimal(values[1]),
                H = parseDecimal(values[2])
            };
        }
    }

    public class PvgisResponse {
        /// <summary>
        /// Nominal power of the PV system (kW).
        /// </summary>
        public decimal NominalPower { get; set; }

        /// <summary>
        /// Estimated losses due to temperature and low irradiance.
        /// </summary>
        public decimal EstTempLowIrrLoss { get; set; }

        /// <summary>
        /// Estimated loss due to angular reflectance effects.
        /// </summary>
        public decimal EstAngReflLoss { get; set; }

        /// <summary>
        /// Other losses (cables, inverter etc.).
        /// </summary>
        public decimal OtherLosses { get; set; }

        /// <summary>
        /// Combined PV system losses.
        /// </summary>
        public decimal CombinedLosses { get; set; }

        /// <summary>
        /// Monthly average.
        /// </summary>
        public Dictionary<string, PvgisResponseMonth> MonthlyAverage { get; set; }

        /// <summary>
        /// Yearly average.
        /// </summary>
        public PvgisResponseMonth YearlyAverage { get; set; }

        /// <summary>
        /// Yearly total.
        /// </summary>
        public PvgisResponseYear YearlyTotal { get; set; }
    }

    public class PvgisResponseMonth {
        public decimal Ed { get; set; }
        public decimal Em { get; set; }
        public decimal Hd { get; set; }
        public decimal Hm { get; set; }
    }

    public class PvgisResponseYear {
        public decimal E { get; set; }
        public decimal H { get; set; }
    }
}