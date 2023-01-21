using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public static class Utils {

	#region UTILITIES

	#region Number Utils

	public static float pixelsPerUnit = 16;

	public static float PixelsToUnits(float value) => PixelsToUnits(value, pixelsPerUnit);
	public static float PixelsToUnits(float value, float ppu) => value / ppu;

	public static float UnitsToPixels(float value) => UnitsToPixels(value, pixelsPerUnit);
	public static float UnitsToPixels(float value, float ppu) => value * ppu;

	public static float ClampWithMax(float value, float max) => value > max ? max : value;
	public static float ClampWithMin(float value, float min) => value < min ? min : value;

	public static float FindTime(float speed, float distance) => distance / speed;
	public static float FindDist(float speed, float time) => speed * time;
	public static float FindSpeed(float distance, float time) => distance / time;

	public static int IndexOfSmallest(float[] values) {
		if (values == null || values.Length == 0)
			throw new Exception("hey what the fuck, give me some values");
		float smallest = values[0];
		int index = 0;
		for (int i = 1; i < values.Length; i++) {
			if (values[i] < smallest) {
				smallest = values[i];
				index = i;
			}
		}
		return index;
	}

	public static int IndexOfLargest(float[] values) {
		if (values == null || values.Length == 0)
			throw new Exception("hey what the fuck, give me some values");
		float largest = values[0];
		int index = 0;
		for (int i = 1; i < values.Length; i++) {
			if (values[i] > largest) {
				largest = values[i];
				index = i;
			}
		}
		return index;
	}

	public static float ExpLerp01(float t, float exp = 10f) => ExpLerp(0, 1, Mathf.Clamp01(t), exp);

	public static float ExpLerp(float a, float b, float t, float exp = 10f) {
		return a + Mathf.Pow(Mathf.InverseLerp(a, b, t), exp) * (b - a);
	}

	public static float AbsMax(float a, float b) {
		if (Mathf.Abs(a) >= Mathf.Abs(b))
			return a;
		return b;
	}

	public static float AbsMin(float a, float b) {
		if (Mathf.Abs(a) <= Mathf.Abs(b))
			return a;
		return b;
	}

	public static float RoundToNearest(float value, float[] snaps) {
		if (snaps == null)
			throw new Exception("um wtf the snaps array u gave me is null :/");
		if (snaps.Length == 0)
			throw new Exception("lmao fucker u gotta put things in the snaps array to snap to");

		float smallestDist = Mathf.Abs(value - snaps[0]);
		int index = 0;
		for (int i = 1; i < snaps.Length; i++) {
			float dist = Mathf.Abs(value - snaps[i]);
			if (dist < smallestDist) {
				smallestDist = dist;
				index = i;
			}
		}
		return snaps[index];
	}

	public static float RoundToNearest(float value, float interval) {
		float intervalAbs = Math.Abs(interval);
		float div = value / intervalAbs;
		float frac = div - (int)div;
		div = (int)div;

		if (frac >= .5f)
			div++;
		else if (frac <= -.5f)
			div--;
		return (int)div * intervalAbs;
	}

	#endregion

	#region Vector Utils

	public static bool IsDistanceGreaterThan(Vector2 a, Vector2 b, float threshold) => IsDistanceGreaterThan(b - a, threshold);

	public static bool IsDistanceLessThan(Vector2 a, Vector2 b, float threshold) => IsDistanceLessThan(b - a, threshold);

	public static bool IsDistanceGreaterThan(Vector2 vector, float threshold) {
		float sqrMagnitude = Mathf.Pow(threshold, 2);
		return sqrMagnitude < vector.sqrMagnitude;
	}

	public static bool IsDistanceLessThan(Vector2 vector, float threshold) {
		float sqrMagnitude = Mathf.Pow(threshold, 2);
		return sqrMagnitude > vector.sqrMagnitude;
	}

	public static Vector2 InverseLerp(Vector2 a, Vector2 b, Vector2 t) {
		return new Vector2(Mathf.InverseLerp(a.x, b.x, t.x), Mathf.InverseLerp(a.y, b.y, t.y));
	}

	public static Vector2 Lerp(Vector2 a, Vector2 b, Vector2 t) {
		return new Vector2(Mathf.Lerp(a.x, b.x, t.x), Mathf.Lerp(a.y, b.y, t.y));
	}

	public static Vector2 FindWeightedCulminativeDisplacement(Vector2[] displacements, float[] weights, bool ignoreZeroVectorDisplacements = true) {
		// normalize the weights
		float maxWeight = 0;
		for (int i = 0; i < weights.Length; i++) {
			if (weights[i] > maxWeight) {
				if (ignoreZeroVectorDisplacements && displacements[i] == Vector2.zero)
					continue;
				maxWeight = weights[i];
			}
		}
		float weightScale = 1f / maxWeight;
		for (int i = 0; i < weights.Length; i++)
			weights[i] *= weightScale;

		Vector2 displacement = displacements[0];
		for (int i = 1; i < displacements.Length; i++)
			displacement += displacements[i] * weights[i];
		return displacement;
	}

	public static Vector2 FindCentroid(Vector2[] vectors, float[] weights) {
		if (vectors == null)
			throw new ArgumentException("ur vector array is null :/");
		if (weights == null)
			throw new ArgumentException("ur weights array is null :/");
		if (vectors.Length != weights.Length)
			throw new ArgumentException("hey fucker the vector n weight arrays cont have the same # of shit in them");
		if (vectors.Length == 0)
			return Vector2.zero;

		Vector2 centroid = vectors[0];
		float currentWeight = weights[0];

		for (int i = 1; i < vectors.Length; i++) {
			if (!(weights[i] == 0 && currentWeight == 0)) {
				float percent = weights[i] / (currentWeight + weights[i]);
				centroid = Vector2.Lerp(centroid, vectors[i], percent);
				currentWeight += weights[i];
			}
		}
		return centroid;
	}

	public static Vector2 Displacement(Vector2 start, Vector2 end) => end - start;
	public static Vector3 Displacement(Vector3 start, Vector3 end) => end - start;

	public static Vector2 Direction(Vector2 start, Vector2 end) => (end - start).normalized;

	public static Vector2 GetRandomDirection() {
		return new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
	}

	public static float SqrMagnitude(Vector2 start, Vector2 end) => (end - start).sqrMagnitude;

	#endregion

	#region Colour Utils

	// based on how unity did it:
	// https://stackoverflow.com/questions/61372498/how-does-mathf-smoothdamp-work-what-is-it-algorithm
	public static Color ColourSmoothDamp(Color current, Color target, ref Vector4 currentVelocity, float smoothTime) =>
		ColourSmoothDamp(current, target, ref currentVelocity, smoothTime, Mathf.Infinity, Time.deltaTime);
	public static Color ColourSmoothDamp(Color current, Color target, ref Vector4 currentVelocity, float smoothTime, float maxSpeed) =>
		ColourSmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, Time.deltaTime);

	public static Color ColourSmoothDamp(Color current, Color target, ref Vector4 currentVelocity, float smoothTime, float maxSpeed, float deltaTime) {
		// Based on Game Programming Gems 4 Chapter 1.10
		smoothTime = Mathf.Max(0.0001f, smoothTime);
		float omega = 2f / smoothTime;

		Vector4 currentColour = new Vector4(current.r, current.g, current.b, current.a);
		Vector4 targetColour = new Vector4(target.r, target.g, target.b, target.a);

		float x = omega * deltaTime;
		float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
		Vector4 change = currentColour - targetColour;
		Vector4 originalTo = targetColour;

		// Clamp maximum speed
		Vector4 maxChange = maxSpeed * smoothTime * Vector4.one;
		change.x = Mathf.Clamp(change.x, -maxChange.x, maxChange.x);
		change.y = Mathf.Clamp(change.y, -maxChange.y, maxChange.y);
		change.z = Mathf.Clamp(change.z, -maxChange.z, maxChange.z);
		change.w = Mathf.Clamp(change.w, -maxChange.w, maxChange.w);

		targetColour = currentColour - change;

		Vector4 temp = (currentVelocity + (omega * change)) * deltaTime;
		currentVelocity = (currentVelocity - omega * temp) * exp;
		Vector4 output = targetColour + (change + temp) * exp;

		// Prevent overshooting
		if (originalTo.x - currentColour.x > 0.0f == output.x > originalTo.x) {
			output.x = originalTo.x;
			currentVelocity.x = (output.x - originalTo.x) / deltaTime;
		}
		if (originalTo.y - currentColour.y > 0.0f == output.y > originalTo.y) {
			output.y = originalTo.y;
			currentVelocity.y = (output.y - originalTo.y) / deltaTime;
		}
		if (originalTo.z - currentColour.z > 0.0f == output.z > originalTo.z) {
			output.z = originalTo.z;
			currentVelocity.z = (output.z - originalTo.z) / deltaTime;
		}
		if (originalTo.w - currentColour.w > 0.0f == output.w > originalTo.w) {
			output.w = originalTo.w;
			currentVelocity.w = (output.w - originalTo.w) / deltaTime;
		}

		return new Color(output.x, output.y, output.z, output.w);
	}

	#endregion

	#region Coroutine Utils

	/// <param name="duration">Duration in seconds.</param>
	/// <param name="realtime">Should use real time or scaled time?</param>
	/// <returns></returns>
	public static IEnumerator PauseableWait(float duration, Func<bool> predicate, bool realtime = false) {
		for (float elapsed = 0; elapsed < duration; elapsed += (realtime?Time.unscaledDeltaTime : Time.deltaTime)) {
			while (predicate.Invoke())
				yield return null;
			yield return null;
		}
	}

	/// <summary>
	/// Call a function every frame for a duration.
	/// </summary>
	/// <param name="duration">How long call the function for.</param>
	/// <param name="action">Function to be invoked and passed the elapsed time.</param>
	/// <returns></returns>
	public static IEnumerator DoOverTime(float duration, Action<float> action, bool callAgainOnComplete = true, Func<bool> pausePredicate = null) {
		for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime) {
			if (pausePredicate != null && pausePredicate.Invoke())
				yield return null;

			action.Invoke(elapsed);
			yield return null;
		}
		if (callAgainOnComplete)
			action.Invoke(duration);
	}
	#endregion

	#region Generic Utils

	public static T DeepCopy<T>(T other) {
		using(MemoryStream ms = new MemoryStream()) {
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(ms, other);
			ms.Position = 0;
			return (T)formatter.Deserialize(ms);
		}
	}

	public static float LinearToLog(float value) {
		if (value == 0)
			return -80;
		return Mathf.Log10(value) * 20f;
	}

	#endregion

	#region Texture Utils

	public static Texture2D GetColourTexture(int width, int height, Color colour) {
		Texture2D tex = new Texture2D(width, height);
		tex.filterMode = FilterMode.Point;
		Color[] colours = new Color[width * height];
		for (int i = 0; i < colours.Length; i++)
			colours[i] = colour;
		tex.SetPixels(0, 0, width, height, colours);
		tex.Apply();
		return tex;
	}

	#endregion

	#endregion

	#region EXTENSION METHODS

	#region String Extensions

	public static int[] IndexesOf(this string s, char character) {
		List<int> indexes = new List<int>();
		for (int i = 0; i < s.Length; i++) {
			if (s[i] == character)
				indexes.Add(i);
		}
		if (indexes.Count == 0)
			return null;
		return indexes.ToArray();
	}

	public static string ReplaceAt(this string s, int index, char newChar) {
		if (index < 0 || index > s.Length)
			throw new System.ArgumentOutOfRangeException();
		return s.Substring(0, index) + newChar + s.Substring(Math.Min(index + 1, s.Length));
	}

	public static string RemoveUntil(this string s, char character) => s.RemoveUntil(0, character);

	public static string RemoveUntil(this string s, int startIndex, char character) {
		return s.Remove(startIndex, s.IndexOf(character) + 1);
	}

	public static string RemoveUntilAny(this string s, char[] anyOf) => s.RemoveUntilAny(0, anyOf);

	public static string RemoveUntilAny(this string s, int startIndex, char[] anyOf) {
		return s.Remove(startIndex, s.IndexOfAny(anyOf, startIndex) + 1);
	}

	public static string SubstringUntil(this string s, char character) => s.SubstringUntil(0, character);

	public static string SubstringUntil(this string s, int startIndex, char character) {
		return s.Substring(startIndex, s.IndexOf(character));
	}

	/// <summary>
	/// Is the character an uppercase letter?
	/// </summary>
	public static bool IsUpper(this char c) => c >= 'A' && c <= 'Z';

	/// <summary>
	/// Is the character a lowercase letter?
	/// </summary>
	public static bool IsLower(this char c) => c >= 'a' && c <= 'z';

	private const int asciiUpperLowerDif = 'a' - 'A';

	public static char ToUpper(this char c) {
		if (c.IsLower())
			c = (char)(c - asciiUpperLowerDif);
		return c;
	}

	public static char ToLower(this char c) {
		if (c.IsUpper())
			c = (char)(c + asciiUpperLowerDif);
		return c;
	}

	public static void IndexesOf(this string s, string value, ref List<int> results) {
		results.Clear();
		for (int i = 0; i < s.Length; i++) {
			int nextIndex = s.IndexOf(value, i);
			if (nextIndex == -1)
				return;
			results.Add(nextIndex);
			i = nextIndex + value.Length - 1;
		}
	}

	public static int[] IndexesOf(this string s, string value) {
		List<int> indexes = new List<int>();
		s.IndexesOf(value, ref indexes);
		return indexes.ToArray();
	}

	/// <summary>
	/// Is the character a letter?
	/// </summary>
	public static bool IsLetter(this char c) => c.IsUpper() || c.IsLower();

	// String - capitalized
	// string - lower
	// STRING - upper
	// 69string - lower
	// string69 - lower
	// s - lower
	// S - upper
	// St - Capitalized
	// sT - none
	// StrinG - none
	// STring - none
	// 69 - none

	public static StringCapitalization GetCapitalization(this string s) {
		if (s == null || s.Length == 0)
			return StringCapitalization.None;

		StringCapitalization capitalization = StringCapitalization.None;
		for (int i = 0; i < s.Length; i++) { // look for just the letters
			if (!s[i].IsLetter())
				continue;
			if (capitalization == StringCapitalization.None && s[i].IsUpper())
				capitalization = StringCapitalization.Capitalized;
			else if (capitalization == StringCapitalization.None && s[i].IsLower())
				capitalization = StringCapitalization.LowerCase;
			else if (capitalization == StringCapitalization.Capitalized && s[i].IsUpper())
				capitalization = StringCapitalization.UpperCase;
			else if (capitalization == StringCapitalization.UpperCase && s[i].IsLower())
				return StringCapitalization.None;
			else if (capitalization == StringCapitalization.LowerCase && s[i].IsUpper())
				return StringCapitalization.None;
		}
		return capitalization;
	}

	public static string SetCapitalization(this string s, StringCapitalization capitalization) {
		if (capitalization == StringCapitalization.LowerCase)
			return s.ToLower();
		else if (capitalization == StringCapitalization.UpperCase)
			return s.ToUpper();
		else if (capitalization == StringCapitalization.Capitalized) {
			for (int i = 0; i < s.Length; i++)
				if (s[i].IsLetter())
					return s.ToLower().ReplaceAt(i, s[i].ToUpper());
		}
		return s;
	}

	public static string MatchCapitalization(this string s, string referenceValue) {
		return s.SetCapitalization(referenceValue.GetCapitalization());
	}

	public static string Capitalize(this string s) {
		if (s == null || s == "")
			return s;
		StringBuilder sb = new StringBuilder();
		bool first = true;

		bool capitalize = false;
		for (int i = 0; i < s.Length; i++) {

			if (capitalize && s[i].IsLetter() || first && s[i].IsLetter()) {
				sb.Append(s[i].ToUpper());
				capitalize = false;
				first = false;

			} else {
				if (s[i] == ' ')
					capitalize = true;
				sb.Append(s[i]);
			}

		}

		return sb.ToString();
	}

	public static bool HasLetters(this string s) {
		foreach (char c in s)
			if (c.IsLetter())
				return true;
		return false;
	}

	public static bool HasUpper(this string s) {
		foreach (char c in s)
			if (c.IsUpper())
				return true;
		return false;
	}

	public static bool HasLower(this string s) {
		foreach (char c in s)
			if (c.IsLower())
				return true;
		return false;
	}

	/// <summary>
	/// /// IS IT IN ALL CAPS?????
	/// </summary>
	public static bool IsAllUpper(this string s) {
		foreach (char c in s)
			if (c.IsLower()) // checks IsLower() instead of IsUpper() so non-letters don't invalidate it
				return false;
		return true;
	}

	/// <summary>
	/// is it all lowercase??
	/// </summary>
	public static bool IsAllLower(this string s) {
		foreach (char c in s)
			if (c.IsUpper()) // checks IsUpper() instead of IsLower() so non-letters don't invalidate it
				return false;
		return true;
	}

	/// <summary>
	/// Trim every string in an array.
	/// </summary>
	public static string[] Trim(this string[] s) {
		for (int i = 0; i < s.Length; i++)
			s[i] = s[i].Trim();
		return s;
	}

	/// <summary>
	/// Puts richtext colour tags around the string.
	/// </summary>
	public static string Colour(this string s, Color colour) {
		string cHex = colour.ToHexRGBA();
		return "<color=#" + cHex + ">" + s + "</color>";
	}

	#endregion

	#region Value Extensions

	/// <summary>
	/// The fractional component of the value.
	/// </summary>
	public static float Frac(this float f) => f - (int)f;

	/// <summary>
	/// The absolute fractional component of the value.
	/// </summary>
	public static float AbsFrac(this float f) => Math.Abs(f - (int)f);

	public static void ClampWithMax(this ref float value, float max) {
		if (value > max)
			value = max;
	}

	public static void ClampWithMin(this ref float value, float min) {
		if (value < min)
			value = min;
	}

	/// <summary>
	/// Is the int even?
	/// </summary>
	public static bool IsEven(this int i) => i % 2 == 0;

	/// <summary>
	/// Is the int odd?
	/// </summary>
	public static bool IsOdd(this int i) => i % 2 != 0;

	/// <summary>
	/// gets u either -1, 0, 1
	/// </summary>
	public static float Direction(this float value) {
		if (value == 0)
			return 0;
		return value / Math.Abs(value);
	}

	/// <summary>
	/// Converts HashSet into an array.
	/// </summary>
	public static T[] ToArray<T>(this HashSet<T> set) {
		T[] array = new T[set.Count];
		int count = 0;
		foreach (T t in set) {
			array[count] = t;
			count++;
		}
		return array;
	}

	public static T GetRandom<T>(this List<T> list) => list[UnityEngine.Random.Range(0, list.Count)];

	public static T GetRandom<T>(this T[] array) => array[UnityEngine.Random.Range(0, array.Length)];

	/// <summary>
	/// Does the array contain the value
	/// </summary>
	public static bool Contains<T>(this T[] array, T value) {
		for (int i = 0; i < array.Length; i++)
			if (array[i].Equals(value))
				return true;
		return false;
	}

	public static string ToString<T>(this T[] array) {
		if (array == null)
			return "null";
		else if (array.Length == 0)
			return "[]";

		StringBuilder sb = new StringBuilder($"[{array[0]}");
		for (int i = 1; i < array.Length; i++)
			sb.Append($", {array[i].ToString()}");
		sb.Append("]");
		return sb.ToString();
	}

	public static void Print<T>(this T[] array) {
		if (array == null) {
			Debug.Log(null);
			return;
		} else if (array.Length == 0) {
			Debug.Log("[]");
			return;
		}

		StringBuilder sb = new StringBuilder($"[{array[0]}");
		for (int i = 1; i < array.Length; i++)
			sb.Append($", {array[i]?.ToString()}");
		sb.Append("]");
		Debug.Log(sb.ToString());
	}

	#endregion

	#region Enum Extensions

	/// <summary>
	/// Returns how many entries there are in the enum.
	/// </summary>
	public static int LengthOfEnum<TEnum>(this TEnum t)where TEnum : struct => System.Enum.GetNames(typeof(TEnum)).Length;

	/// <summary>
	/// Inserts spaces in between words of an enum.
	/// </summary>
	public static string MakeEnumReadable<TEnum>(this TEnum t)where TEnum : struct, IConvertible {
		string entry = t.ToString();
		for (int i = 1; i < entry.Length; i++) {
			if (entry[i].IsUpper()) {
				entry = entry.Insert(i, " ");
				i++;
			}
		}
		return entry;
	}

	#endregion

	#region Vector Extensions

	public static Vector2 Add(this ref Vector2 v, float x, float y) => v + new Vector2(x, y);

	public static Vector3 ToV3(this Vector2 v, float z) => new Vector3(v.x, v.y, z);

	public static Vector2 Abs(this Vector2 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
	public static Vector2Int Abs(this Vector2Int v) => new Vector2Int(Mathf.Abs(v.x), Mathf.Abs(v.y));
	public static Vector3 Abs(this Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	public static Vector3Int Abs(this Vector3Int v) => new Vector3Int(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

	public static Vector2 Round(this ref Vector2 v) => v = v.Rounded();
	public static Vector3 Round(this ref Vector3 v) => v = v.Rounded();
	public static Vector2 Rounded(this Vector2 v) => new Vector2(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
	public static Vector3 Rounded(this Vector3 v) => new Vector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
	public static Vector2Int RoundedToInt(this Vector2 v) => new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
	public static Vector3Int RoundedToInt(this Vector3 v) => new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));

	public static Vector2 Floor(this ref Vector2 v) => v = v.Floored();
	public static Vector3 Floor(this ref Vector3 v) => v = v.Floored();
	public static Vector2 Floored(this Vector2 v) => new Vector2(Mathf.Floor(v.x), Mathf.Floor(v.y));
	public static Vector3 Floored(this Vector3 v) => new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));

	public static Vector2 Ceil(this ref Vector2 v) => v = v.Ceiled();
	public static Vector3 Ceil(this ref Vector3 v) => v = v.Ceiled();
	public static Vector2 Ceiled(this Vector2 v) => new Vector2(Mathf.Ceil(v.x), Mathf.Ceil(v.y));
	public static Vector3 Ceiled(this Vector3 v) => new Vector3(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z));

	public static Vector2 Clamp(this ref Vector2 v, Vector2 a, Vector2 b) {
		v.x = Mathf.Clamp(v.x, Mathf.Min(a.x, b.x), Mathf.Max(a.x, b.x));
		v.y = Mathf.Clamp(v.y, Mathf.Min(a.y, b.y), Mathf.Max(a.y, b.y));
		return v;
	}

	public static Vector2 Mul(this Vector2 v, Vector2 scale) => new Vector2(v.x * scale.x, v.y * scale.y);
	public static Vector3 Mul(this Vector3 v, Vector3 scale) => new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);

	public static Vector2 SetMagnitude(this Vector2 v, float magnitude) => v.normalized * magnitude;

	public static Vector2 ClampMagnitude(this Vector2 v, float maxLength) => Vector2.ClampMagnitude(v, maxLength);

	public static Vector2 YX(this Vector2 v) => new Vector2(v.y, v.y);
	public static Vector3 XZY(this Vector3 v) => new Vector3(v.x, v.z, v.y); // flip y z
	public static Vector3 YXZ(this Vector3 v) => new Vector3(v.y, v.x, v.z); // flip x y
	public static Vector3 YZX(this Vector3 v) => new Vector3(v.y, v.z, v.x); // flip x y z
	public static Vector3 ZXY(this Vector3 v) => new Vector3(v.z, v.x, v.y); // flip x y z
	public static Vector3 ZYX(this Vector3 v) => new Vector3(v.z, v.y, v.x); // flip x z

	#endregion

	#region Colour Extensions

	public static Color WithAlpha(this Color c, float alpha) {
		c.a = alpha;
		return c;
	}

	public static string ToHexRGBA(this Color c) {
		string r = ((int)Mathf.Lerp(0, 255, c.r)).ToString("X2");
		string g = ((int)Mathf.Lerp(0, 255, c.g)).ToString("X2");
		string b = ((int)Mathf.Lerp(0, 255, c.b)).ToString("X2");
		string a = ((int)Mathf.Lerp(0, 255, c.a)).ToString("X2");
		return r + g + b + a;
	}

	public static Gradient AsGradient(this Color c) {
		Gradient grad = new Gradient();
		grad.colorKeys = new GradientColorKey[] {
			new GradientColorKey(c, 0),
				new GradientColorKey(c, 1),
		};
		grad.alphaKeys = new GradientAlphaKey[] {
			new GradientAlphaKey(c.a, 0),
				new GradientAlphaKey(c.a, 1),
		};
		return grad;
	}

	#endregion

	#region Bounds Extensions

	public static bool IsPointInside(this Bounds bounds, Vector2 point) =>
		point.x <= bounds.max.x && point.x >= bounds.min.x && point.y <= bounds.max.y && point.y >= bounds.min.y;

	public static Vector3 GetPercentInBounds(this Bounds bounds, Vector3 percent) {
		return bounds.min + bounds.size.Mul(percent);
	}

	public static Vector2 TopLeft(this Bounds bounds) => new Vector2(bounds.min.x, bounds.max.y);
	public static Vector2 TopMiddle(this Bounds bounds) => new Vector2(bounds.center.x, bounds.max.y);
	public static Vector2 TopRight(this Bounds bounds) => bounds.max;

	public static Vector2 CenterLeft(this Bounds bounds) => new Vector2(bounds.min.x, bounds.center.y);
	public static Vector2 CenterMiddle(this Bounds bounds) => bounds.center;
	public static Vector2 CenterRight(this Bounds bounds) => new Vector2(bounds.max.x, bounds.center.y);

	public static Vector2 BottomLeft(this Bounds bounds) => bounds.min;
	public static Vector2 BottomMiddle(this Bounds bounds) => new Vector2(bounds.center.x, bounds.min.y);
	public static Vector2 BottomRight(this Bounds bounds) => new Vector2(bounds.max.x, bounds.min.y);

	#endregion

	#region Rect Extensions

	/// <summary>
	/// Gets you a Rect in where the min value is *actually* at the bottom of the rectange. ffs
	/// </summary>
	public static Rect MinMaxRect(Vector2 min, Vector2 max) => Rect.MinMaxRect(min.x, max.y, max.x, min.y);

	public static Vector2 GetPercentInRect(this Rect rect, Vector2 percent) {
		return rect.min + rect.size * percent;
	}

	#endregion

	#region RectTransform Extensions

	public static Vector2 WorldPosAnchor(this RectTransform rt, Canvas canvas) {
		return (Vector2)rt.position - (rt.anchoredPosition * canvas.scaleFactor);
	}

	/// <summary>
	/// Returns the anchor position needed to allign two RectTransforms at their pivots.
	/// </summary>
	/// <param name="selfRT">The RectTransform to be moved.</param>
	/// <param name="targetRT">The RectTransform target.</param>
	public static Vector2 AnchorPosOfSiblingRT(RectTransform selfRT, RectTransform targetRT) => AnchorPosOfSiblingRT(selfRT, targetRT, selfRT.pivot, targetRT.pivot);

	/// <summary>
	/// Returns the anchor position needed to allign two RectTransforms at their given localized positions.
	/// </summary>
	/// <param name="selfRT">The RectTransform to be moved.</param>
	/// <param name="targetRT">The RectTransform target.</param>
	/// <param name="selfPos">The normalized local position inside the moving RectTransform.</param>
	/// <param name="targetPos">The normalized local position inside the target RectTransform.</param>
	public static Vector2 AnchorPosOfSiblingRT(RectTransform selfRT, RectTransform targetRT, Vector2 selfPos, Vector2 targetPos) {
		Vector2 selfOffset = GetOffset(selfRT, selfPos);
		Vector2 targetOffset = GetOffset(targetRT, targetPos);

		Vector2 anchorRelativeToPos = selfRT.anchoredPosition - (Vector2)selfRT.localPosition;
		return (Vector2)targetRT.localPosition + anchorRelativeToPos - selfOffset + targetOffset;

		Vector2 GetOffset(RectTransform rt, Vector2 normalizedPos) {
			return (normalizedPos - rt.pivot) * rt.rect.size;
		}
	}

	public static Rect GetLocalRect(this RectTransform rt) => rt.rect;

	public static Rect GetWorldRect(this RectTransform rt) {
		Vector3[] points = new Vector3[4];
		rt.GetWorldCorners(points);
		return GetRectFromCorners(points);
	}

	public static Bounds GetWorldBounds(this RectTransform rt) {
		Vector3[] points = new Vector3[4];
		rt.GetWorldCorners(points);
		return GetBoundsFromCorners(points);
	}

	public static Bounds GetLocalBounds(this RectTransform rt) {
		Vector3[] points = new Vector3[4];
		rt.GetLocalCorners(points);
		return GetBoundsFromCorners(points);
	}

	private static Bounds GetBoundsFromCorners(Vector3[] points) {
		return new Bounds(new Vector2(
			(points[0].x + points[2].x) / 2f,
			(points[0].y + points[2].y) / 2f
		), new Vector2(
			points[3].x - points[0].x,
			points[1].y - points[0].y
		));
	}

	private static Rect GetRectFromCorners(Vector3[] points) {
		return new Rect(
			points[0].x,
			points[0].y,
			points[3].x - points[0].x,
			points[1].y - points[0].y
		);
	}

	#endregion

	#region Camera Bounds

	public static Bounds GetBounds(this Camera cam) {
		return new Bounds(cam.transform.position, new Vector2(cam.aspect * cam.orthographicSize, cam.orthographicSize) * 2f);
	}

	#endregion

	#region MonoBehaviour Extensions

	public static Transform GetGrandChild(this Transform trans) {
		if (trans.childCount == 0)
			return trans;
		return trans.GetChild(0).GetGrandChild();
	}

	#endregion

	#region Array Extensions

	/// <summary>
	/// Returns a string showing the ToString() for each element in the list.
	/// </summary>
	public static string ListValues<T>(this T[] array) {
		StringBuilder sb = new StringBuilder();
		const string separator = ", ";
		for (int i = 0; i < array.Length; i++) {
			if (i > 0)
				sb.Append(separator);
			sb.Append(array[i]?.ToString() ?? "null");
		}
		return sb.ToString();
	}

	#endregion

	#region List Extentions

	/// <summary>
	/// Returns a string showing the ToString() for each element in the list.
	/// </summary>
	public static string ListValues<T>(this List<T> list) {
		StringBuilder sb = new StringBuilder("[");
		if (list.Count > 0) {
			sb.Append(list[0].ToString());
			if (list.Count > 1)
				for (int i = 1; i < list.Count; i++)
					sb.Append(list[i].ToString());
		}
		sb.Append("]");
		return sb.ToString();
	}

	/// <summary>
	/// Removes the element from the list and inserts it back into the list at the new index.
	/// </summary>
	public static void Move<T>(this List<T> list, int elementIndex, int newIndex) {
		list.Move(list[elementIndex], newIndex);
	}

	/// <summary>
	/// Removes the element from the list and inserts it back into the list at the new index.
	/// </summary>
	public static void Move<T>(this List<T> list, T element, int newIndex) {
		list.Remove(element);
		list.Insert(newIndex, element);
	}

	#endregion

	#endregion

}

public enum StringCapitalization {
	None,
	Capitalized,
	UpperCase,
	LowerCase
}