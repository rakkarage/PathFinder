using UnityEngine;
namespace ca.HenrySoftware.Rage
{
	public static partial class Utility
	{
		public static class Random
		{
			private static System.Random _random = new System.Random();
			public static int Next(int max) => _random.Next(max);
			public static int Next(int min, int max) => _random.Next(min, max);
			public static int NextEven(int min, int max) => Next(min / 2, max / 2) * 2;
			public static int NextOdd(int min, int max) => Next(min, max) + 1;
			public static double NextDouble() => _random.NextDouble();
			public static double NextDouble(double max) => NextDouble() * max;
			public static double NextDouble(double min, double max) => min + NextDouble() * (max - min);
			public static float NextFloat() => (float)NextDouble();
			public static float NextFloat(float max) => (float)NextDouble(max);
			public static float NextFloat(float min, float max) => (float)NextDouble(min, max);
			public static bool NextBool() => NextDouble() > 0.5;
			public static bool NextPercent(double target) => NextDouble() < target;
			public static Color NextColor() => new Color(NextFloat(), NextFloat(), NextFloat());
		}
	}
}
