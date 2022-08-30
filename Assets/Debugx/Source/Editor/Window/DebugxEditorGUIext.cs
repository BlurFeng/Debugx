using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DebugxU3D
{
	public static class GUILayoutx
	{
		public static bool ButtonGreen(string text)
		{
			GUIUtilityx.PushTint(Color.green);
			bool press = GUILayout.Button(text);
			GUIUtilityx.PopTint();

			return press;
		}

		public static bool ButtonRed(string text)
		{
			GUIUtilityx.PushTint(Color.red);
			bool press = GUILayout.Button(text);
			GUIUtilityx.PopTint();

			return press;
		}
	}

	//GUI扩展工具
	//有从AstarPathfindingProject插件拿过来的GUI绘制类

	public static class GUIUtilityx
	{
		static Stack<Color> colors = new Stack<Color>();

		public static void PushTint(Color tint)
		{
			colors.Push(GUI.color);
			GUI.color *= tint;
		}

		public static void PopTint()
		{
			GUI.color = colors.Pop();
		}
	}

	/// <summary>
	/// Editor helper for hiding and showing a group of GUI elements.
	/// Call order in OnInspectorGUI should be:
	/// - Begin
	/// - Header/HeaderLabel (optional)
	/// - BeginFade
	/// - [your gui elements] (if BeginFade returns true)
	/// - End
	/// </summary>
	public class FadeArea
	{
		Rect lastRect;
		float value;
		float lastUpdate;
		GUIStyle labelStyle;
		GUIStyle areaStyle;
		bool visible;
		EditorWindow editorWindow;

		/// <summary>
		/// Is this area open.
		/// This is not the same as if any contents are visible, use <see cref="BeginFade"/> for that.
		/// </summary>
		public bool open;

		/// <summary>Animate dropdowns when they open and close</summary>
		public static bool fancyEffects = true;
		const float animationSpeed = 100f;

		public FadeArea(EditorWindow editor, bool open, GUIStyle areaStyle, GUIStyle labelStyle = null)
		{
			this.editorWindow = editor;

			this.areaStyle = areaStyle;
			this.labelStyle = labelStyle;
			visible = this.open = open;
			value = open ? 1 : 0;
		}

		void Tick()
		{
			if (Event.current.type == EventType.Repaint)
			{
				float deltaTime = Time.realtimeSinceStartup - lastUpdate;

				// Right at the start of a transition the deltaTime will
				// not be reliable, so use a very small value instead
				// until the next repaint
				if (value == 0f || value == 1f) deltaTime = 0.001f;
				deltaTime = Mathf.Clamp(deltaTime, 0.00001F, 0.1F);

				// Larger regions fade slightly slower
				deltaTime /= Mathf.Sqrt(Mathf.Max(lastRect.height, 100));

				lastUpdate = Time.realtimeSinceStartup;


				float targetValue = open ? 1F : 0F;
				if (!Mathf.Approximately(targetValue, value))
				{
					value += deltaTime * animationSpeed * Mathf.Sign(targetValue - value);
					value = Mathf.Clamp01(value);
					editorWindow.Repaint();

					if (!fancyEffects)
					{
						value = targetValue;
					}
				}
				else
				{
					value = targetValue;
				}
			}
		}

		public void Begin()
		{
			if (areaStyle != null)
			{
				lastRect = EditorGUILayout.BeginVertical(areaStyle);
			}
			else
			{
				lastRect = EditorGUILayout.BeginVertical();
			}
		}

		public void HeaderLabel(string label)
		{
			GUILayout.Label(label, labelStyle);
		}

		public void Header(string label)
		{
			Header(label, ref open);
		}

		public void Header(string label, int width)
		{
			Header(label, ref open, width);
		}

		public void Header(string label, ref bool open, int width = -1)
		{
			bool press = false;
			if(width > 0)
            {
				press = GUILayout.Button(label, labelStyle, GUILayout.Width(width));
			}
			else
            {
				press = GUILayout.Button(label, labelStyle);
			}

			if (press)
			{
				open = !open;
				editorWindow.Repaint();
			}
			this.open = open;
		}

		/// <summary>Hermite spline interpolation</summary>
		static float Hermite(float start, float end, float value)
		{
			return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
		}

		public bool BeginFade()
		{
			var hermite = Hermite(0, 1, value);

			visible = EditorGUILayout.BeginFadeGroup(hermite);
			GUIUtilityx.PushTint(new Color(1, 1, 1, hermite));
			Tick();

			// Another vertical group is necessary to work around
			// a kink of the BeginFadeGroup implementation which
			// causes the padding to change when value!=0 && value!=1
			EditorGUILayout.BeginVertical();

			return visible;
		}

		public void End()
		{
			EditorGUILayout.EndVertical();

			if (visible)
			{
				// Some space that cannot be placed in the GUIStyle unfortunately
				GUILayout.Space(4);
			}

			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.EndVertical();
			GUIUtilityx.PopTint();
		}
	}
}