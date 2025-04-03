#if UNITY_EDITOR && UNITY_STANDALONE_WIN

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System;

public class InstanceIdViewerWindow : EditorWindow
{
    private string keywordInput = "Camera,Webcam,USB Video";
    private List<(string name, string id)> results = new();
    private Vector2 scroll;

    [MenuItem("Tools/InstanceId Viewer")]
    public static void ShowWindow()
    {
        _ = GetWindow<InstanceIdViewerWindow>("Camera InstanceId Viewer");
    }

    private void OnGUI()
    {
        GUILayout.Label("検索キーワード（カンマ区切り）", EditorStyles.boldLabel);
        keywordInput = EditorGUILayout.TextField("Keywords", keywordInput);

        if (GUILayout.Button("デバイス検索"))
        {
            results = SearchDevices(keywordInput);
        }

        GUILayout.Space(10);
        GUILayout.Label($"検索結果：{results.Count} 件", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach ((string name, string id) in results)
        {
            EditorGUILayout.LabelField("Name", name);
            _ = EditorGUILayout.TextField("InstanceId", id);
            EditorGUILayout.Space(8);
        }
        EditorGUILayout.EndScrollView();
    }

    List<(string, string)> SearchDevices(string keywordString)
    {
        var keywords = keywordString.Split(',');
        for (int i = 0; i < keywords.Length; i++)
            keywords[i] = Regex.Escape(keywords[i].Trim());

        string matchPattern = string.Join("|", keywords);
        string psCommand = $"chcp 65001 >$null; Get-PnpDevice | Where-Object {{$_.FriendlyName -match '{matchPattern}'}} | Select-Object FriendlyName,InstanceId";


        var result = new List<(string, string)>();

        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            using (var process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                // 1～2行目はヘッダー行なのでスキップ
                for (int i = 2; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.Length < 10) continue;

                    // 文字列の中で空白が複数連続している場所で区切る（カラム分け）
                    var parts = Regex.Split(line, @"\s{2,}");
                    if (parts.Length >= 2)
                    {
                        string name = parts[0].Trim();
                        string id = parts[1].Trim();
                        result.Add((name, id));
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("PowerShell 実行に失敗しました: " + e.Message);
        }

        return result;
    }


}

#endif