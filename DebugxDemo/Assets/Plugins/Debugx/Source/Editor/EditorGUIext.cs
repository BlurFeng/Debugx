using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SettingsProvider = UnityEditor.SettingsProvider;

namespace DebugxLog
{
	public static class GUILayoutx
	{
		public static bool ButtonGreen(string text, string tooltip = "")
		{
			GUIUtilityx.PushTint(Color.green);
			bool press = GUILayout.Button(new GUIContent(text, tooltip));
			GUIUtilityx.PopTint();

			return press;
		}

		public static bool ButtonRed(string text, string tooltip = "")
		{
			GUIUtilityx.PushTint(Color.red);
			bool press = GUILayout.Button(new GUIContent(text, tooltip));
			GUIUtilityx.PopTint();

			return press;
		}

        public static bool ButtonYellow(string text, string tooltip = "")
        {
            GUIUtilityx.PushTint(Color.yellow);
            bool press = GUILayout.Button(new GUIContent(text, tooltip));
            GUIUtilityx.PopTint();

            return press;
        }

        public static bool Toggle(string label, string tooltip, bool value, float width = 250f)
		{
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), GUILayout.Width(width));
            bool change = EditorGUILayout.Toggle(value);
            EditorGUILayout.EndHorizontal();

			return change;
        }

        public static int IntField(string label, string tooltip, int value, float width = 250f)
		{
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), GUILayout.Width(width));
            int change = EditorGUILayout.IntField(value);
            EditorGUILayout.EndHorizontal();

            return change;
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
        readonly UnityEngine.GUIStyle labelStyle;
        readonly UnityEngine.GUIStyle areaStyle;
		bool visible;
		readonly EditorWindow editorWindow;
        readonly SettingsProvider settingsProvider;
		readonly bool immediately;
        public float beginSpace = 1.5f;

        /// <summary>
        /// Is this area open.
        /// This is not the same as if any contents are visible, use <see cref="BeginFade"/> for that.
        /// </summary>
        public bool open;

		/// <summary>Animate dropdowns when they open and close</summary>
		public static bool fancyEffects = true;
		const float animationSpeed = 100f;

		public FadeArea(SettingsProvider settingsProvider, bool open)
		{
			new FadeArea(settingsProvider, open, GUIStyle.Get.AreaStyle_1, GUIStyle.Get.LabelStyle_FadeAreaHeader);
        }

		public FadeArea(EditorWindow editor, bool open, UnityEngine.GUIStyle areaStyle, UnityEngine.GUIStyle labelStyle = null, float beginSpace = 1.5f, bool immediately = false)
		{
			this.editorWindow = editor;

			this.areaStyle = areaStyle;
			this.labelStyle = labelStyle;
			visible = this.open = open;
			value = open ? 1 : 0;
			this.beginSpace = beginSpace;
            this.immediately = immediately;
        }

        public FadeArea(SettingsProvider settingsProvider, bool open, UnityEngine.GUIStyle areaStyle, UnityEngine.GUIStyle labelStyle = null, float beginSpace = 1.5f, bool immediately = false)
        {
			this.settingsProvider = settingsProvider;

            this.areaStyle = areaStyle;
            this.labelStyle = labelStyle;
            visible = this.open = open;
            value = open ? 1 : 0;
            this.beginSpace = beginSpace;
            this.immediately = immediately;
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

                    settingsProvider?.Repaint();
                    editorWindow?.Repaint();

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

            EditorGUILayout.Space(beginSpace);
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
			bool changedCached = GUI.changed;

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
                settingsProvider?.Repaint();
                editorWindow?.Repaint();
			}
			this.open = open;
			if (immediately) value = open ? 1f : 0f;

			if (!changedCached) GUI.changed = changedCached;//开关FadeArea排除Changed判断
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