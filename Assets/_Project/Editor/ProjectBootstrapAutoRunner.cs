using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GuardiaoDosCristaisEditor
{
    [InitializeOnLoad]
    public static class ProjectBootstrapAutoRunner
    {
        // Dispara uma regeneracao unica quando o marcador existe.
        private const string MarkerRelativePath = "Temp/guardiao-bootstrap.pending";
        private static bool isQueued;

        static ProjectBootstrapAutoRunner()
        {
            EditorApplication.update += PollForBootstrapMarker;
        }

        private static void PollForBootstrapMarker()
        {
            if (isQueued || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            string markerPath = GetMarkerPath();
            if (!File.Exists(markerPath))
                return;

            File.Delete(markerPath);
            isQueued = true;

            EditorApplication.delayCall += () =>
            {
                try
                {
                    Debug.Log("[GuardiaoDosCristais] Regeneracao automatica iniciada.");
                    ProjectBootstrap.BuildCompleteProject();
                    Debug.Log("[GuardiaoDosCristais] Regeneracao automatica concluida.");
                }
                catch (Exception ex)
                {
                    Debug.LogError("[GuardiaoDosCristais] Falha na regeneracao automatica: " + ex);
                }
                finally
                {
                    isQueued = false;
                }
            };
        }

        private static string GetMarkerPath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            return Path.Combine(projectRoot, MarkerRelativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
