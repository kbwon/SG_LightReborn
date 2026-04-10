using System;
using UnityEditor;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [CustomEditor(typeof(ColorCorrectionLookup))]
    public class ColorCorrectionLookupEditor : Editor
    {
        SerializedObject serObj;
        private Texture2D tempClutTex2D;

        private void OnEnable()
        {
            serObj = new SerializedObject(target);
        }

        public override void OnInspectorGUI()
        {
            serObj.Update();

            EditorGUILayout.LabelField("Converts textures into color lookup volumes (for grading)", EditorStyles.miniLabel);

            // LUT 텍스처 선택 필드
            tempClutTex2D = EditorGUILayout.ObjectField(" Based on", tempClutTex2D, typeof(Texture2D), false) as Texture2D;

            if (tempClutTex2D == null)
            {
                var fallbackTex = AssetDatabase.LoadMainAssetAtPath(((ColorCorrectionLookup)target).basedOnTempTex) as Texture2D;
                if (fallbackTex) tempClutTex2D = fallbackTex;
            }

            Texture2D tex = tempClutTex2D;

            if (tex && ((ColorCorrectionLookup)target).basedOnTempTex != AssetDatabase.GetAssetPath(tex))
            {
                EditorGUILayout.Space();

                if (!((ColorCorrectionLookup)target).ValidDimensions(tex))
                {
                    EditorGUILayout.HelpBox("Invalid texture dimensions!\nPick another texture or adjust dimension to e.g. 256x16.", MessageType.Warning);
                }
                else if (GUILayout.Button("Convert and Apply"))
                {
                    string path = AssetDatabase.GetAssetPath(tex);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

                    if (textureImporter != null)
                    {
                        bool changed = false;

                        // 읽기 가능 설정
                        if (!textureImporter.isReadable)
                        {
                            textureImporter.isReadable = true;
                            changed = true;
                        }

                        // Mipmap 비활성화
                        if (textureImporter.mipmapEnabled)
                        {
                            textureImporter.mipmapEnabled = false;
                            changed = true;
                        }

                        // 포맷 설정 변경
                        var platformSettings = textureImporter.GetDefaultPlatformTextureSettings();
                        if (platformSettings.format != TextureImporterFormat.RGBA32)
                        {
                            platformSettings.format = TextureImporterFormat.RGBA32;
                            textureImporter.SetPlatformTextureSettings(platformSettings);
                            changed = true;
                        }

                        if (changed)
                        {
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        }
                    }

                    ((ColorCorrectionLookup)target).Convert(tex, path);
                }
            }

            if (!string.IsNullOrEmpty(((ColorCorrectionLookup)target).basedOnTempTex))
            {
                EditorGUILayout.HelpBox("Using " + ((ColorCorrectionLookup)target).basedOnTempTex, MessageType.Info);
                var previewTex = AssetDatabase.LoadMainAssetAtPath(((ColorCorrectionLookup)target).basedOnTempTex) as Texture2D;
                if (previewTex)
                {
                    Rect r = GUILayoutUtility.GetRect(128, 20, GUILayout.ExpandWidth(true));
                    GUI.DrawTexture(r, previewTex, ScaleMode.ScaleToFit);
                    GUILayout.Space(4);
                }
            }

            serObj.ApplyModifiedProperties();
        }
    }
}
