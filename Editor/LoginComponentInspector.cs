// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Collections.Generic;
using GameFrameX.Editor;
using GameFrameX.Login.Runtime;
using GameFrameX.Runtime;
using UnityEditor;
using UnityEngine;

namespace GameFrameX.Login.Editor
{
    /// <summary>
    /// 登录组件 Inspector
    /// </summary>
    [CustomEditor(typeof(LoginComponent))]
    public sealed class LoginComponentInspector : ComponentTypeComponentInspector
    {
        private GUIContent m_componentAndroidTypeGUIContent = new GUIContent("Android平台登录组件");
        private GUIContent m_componentIOSTypeGUIContent = new GUIContent("iOS平台登录组件");
        private GUIContent m_componentWebGLTypeGUIContent = new GUIContent("WebGL平台登录组件");
        private GUIContent m_componentPCTypeGUIContent = new GUIContent("PC平台登录组件 (Windows/Linux)");
        private GUIContent m_componentMacTypeGUIContent = new GUIContent("macOS平台登录组件");

        private SerializedProperty ComponentAndroidType = null;
        private string[] ComponentAndroidTypeNames = null;
        private int ComponentAndroidTypeNameIndex = 0;

        private SerializedProperty ComponentIOSType = null;
        private string[] ComponentIOSTypeNames = null;
        private int ComponentIOSTypeNameIndex = 0;

        private SerializedProperty ComponentWebGLType = null;
        private string[] ComponentWebGLTypeNames = null;
        private int ComponentWebGLTypeNameIndex = 0;

        private SerializedProperty ComponentPCType = null;
        private string[] ComponentPCTypeNames = null;
        private int ComponentPCTypeNameIndex = 0;

        private SerializedProperty ComponentMacType = null;
        private string[] ComponentMacTypeNames = null;
        private int ComponentMacTypeNameIndex = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode & Application.isPlaying);
            {
                EditorGUILayout.LabelField(m_componentAndroidTypeGUIContent);
                int componentAndroidTypeNameIndex = EditorGUILayout.Popup("Component Android Type", ComponentAndroidTypeNameIndex, ComponentAndroidTypeNames);
                if (componentAndroidTypeNameIndex != ComponentAndroidTypeNameIndex)
                {
                    ComponentAndroidTypeNameIndex = componentAndroidTypeNameIndex;
                    ComponentAndroidType.stringValue = componentAndroidTypeNameIndex <= 0 ? null : ComponentAndroidTypeNames[componentAndroidTypeNameIndex];
                }

                EditorGUILayout.LabelField(m_componentIOSTypeGUIContent);
                int componentIOSTypeNameIndex = EditorGUILayout.Popup("Component IOS Type", ComponentIOSTypeNameIndex, ComponentIOSTypeNames);
                if (componentIOSTypeNameIndex != ComponentIOSTypeNameIndex)
                {
                    ComponentIOSTypeNameIndex = componentIOSTypeNameIndex;
                    ComponentIOSType.stringValue = componentIOSTypeNameIndex <= 0 ? null : ComponentIOSTypeNames[componentIOSTypeNameIndex];
                }

                EditorGUILayout.LabelField(m_componentWebGLTypeGUIContent);
                int componentWebGLTypeNameIndex = EditorGUILayout.Popup("Component WebGL Type", ComponentWebGLTypeNameIndex, ComponentWebGLTypeNames);
                if (componentWebGLTypeNameIndex != ComponentWebGLTypeNameIndex)
                {
                    ComponentWebGLTypeNameIndex = componentWebGLTypeNameIndex;
                    ComponentWebGLType.stringValue = componentWebGLTypeNameIndex <= 0 ? null : ComponentWebGLTypeNames[componentWebGLTypeNameIndex];
                }

                EditorGUILayout.LabelField(m_componentPCTypeGUIContent);
                int componentPCTypeNameIndex = EditorGUILayout.Popup("Component PC Type", ComponentPCTypeNameIndex, ComponentPCTypeNames);
                if (componentPCTypeNameIndex != ComponentPCTypeNameIndex)
                {
                    ComponentPCTypeNameIndex = componentPCTypeNameIndex;
                    ComponentPCType.stringValue = componentPCTypeNameIndex <= 0 ? null : ComponentPCTypeNames[componentPCTypeNameIndex];
                }

                EditorGUILayout.LabelField(m_componentMacTypeGUIContent);
                int componentMacTypeNameIndex = EditorGUILayout.Popup("Component Mac Type", ComponentMacTypeNameIndex, ComponentMacTypeNames);
                if (componentMacTypeNameIndex != ComponentMacTypeNameIndex)
                {
                    ComponentMacTypeNameIndex = componentMacTypeNameIndex;
                    ComponentMacType.stringValue = componentMacTypeNameIndex <= 0 ? null : ComponentMacTypeNames[componentMacTypeNameIndex];
                }
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void Enable()
        {
            base.Enable();
            ComponentAndroidType = serializedObject.FindProperty("m_componentAndroidType");
            ComponentIOSType = serializedObject.FindProperty("m_componentIOSType");
            ComponentWebGLType = serializedObject.FindProperty("m_componentWebGLType");
            ComponentPCType = serializedObject.FindProperty("m_componentPCType");
            ComponentMacType = serializedObject.FindProperty("m_componentMacType");
            RefreshTypeNames(ComponentAndroidType, ref ComponentAndroidTypeNames, ref ComponentAndroidTypeNameIndex, typeof(ILoginManager));
            RefreshTypeNames(ComponentIOSType, ref ComponentIOSTypeNames, ref ComponentIOSTypeNameIndex, typeof(ILoginManager));
            RefreshTypeNames(ComponentWebGLType, ref ComponentWebGLTypeNames, ref ComponentWebGLTypeNameIndex, typeof(ILoginManager));
            RefreshTypeNames(ComponentPCType, ref ComponentPCTypeNames, ref ComponentPCTypeNameIndex, typeof(ILoginManager));
            RefreshTypeNames(ComponentMacType, ref ComponentMacTypeNames, ref ComponentMacTypeNameIndex, typeof(ILoginManager));
        }

        protected override void RefreshTypeNames()
        {
            RefreshComponentTypeNames(typeof(ILoginManager));
        }

        private void RefreshTypeNames(SerializedProperty property, ref string[] names, ref int index, System.Type type)
        {
            List<string> managerTypeNames = new List<string>
            {
                NoneOptionName
            };

            managerTypeNames.AddRange(Utility.Assembly.GetRuntimeTypeNames(type));
            names = managerTypeNames.ToArray();
            index = 0;
            if (!property.stringValue.IsNullOrEmpty())
            {
                index = managerTypeNames.IndexOf(property.stringValue);
                if (index <= 0)
                {
                    index = 0;
                    property.stringValue = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
