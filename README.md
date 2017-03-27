# PVGIS

Example:

```csharp
var result = PVGIS.PvgisWrapper.Get(63.43401282856549M, 10.41379988193512M)
```

Should result in something like this:

```js
{
	NominalPower: 1,
	EstTempLowIrrLoss: 7,
	EstAngReflLoss: 3.3,
	OtherLosses: 14,
	CombinedLosses: 22.6,
	MonthlyAverage: {
		Jan: {
			Ed: 0.4,
			Em: 12.3,
			Hd: 0.46,
			Hm: 14.4
		},
		Feb: {
			Ed: 1.35,
			Em: 37.8,
			Hd: 1.6,
			Hm: 44.7
		},
		Mar: {
			Ed: 2.38,
			Em: 73.9,
			Hd: 2.93,
			Hm: 90.7
		},
		Apr: {
			Ed: 3.32,
			Em: 99.5,
			Hd: 4.23,
			Hm: 127
		},
		May: {
			Ed: 3.83,
			Em: 119,
			Hd: 5.04,
			Hm: 156
		},
		Jun: {
			Ed: 3.98,
			Em: 119,
			Hd: 5.34,
			Hm: 160
		},
		Jul: {
			Ed: 3.59,
			Em: 111,
			Hd: 4.86,
			Hm: 151
		},
		Aug: {
			Ed: 2.94,
			Em: 91.2,
			Hd: 3.94,
			Hm: 122
		},
		Sep: {
			Ed: 2.09,
			Em: 62.6,
			Hd: 2.67,
			Hm: 80.2
		},
		Oct: {
			Ed: 1.23,
			Em: 38.1,
			Hd: 1.51,
			Hm: 46.9
		},
		Nov: {
			Ed: 0.58,
			Em: 17.5,
			Hd: 0.69,
			Hm: 20.8
		},
		Dec: {
			Ed: 0.21,
			Em: 6.67,
			Hd: 0.26,
			Hm: 7.96
		}
	},
	YearlyAverage: {
		Ed: 2.16,
		Em: 65.7,
		Hd: 2.8,
		Hm: 85.1
	},
	YearlyTotal: {
		E: 789,
		H: 1020
	}
}
```