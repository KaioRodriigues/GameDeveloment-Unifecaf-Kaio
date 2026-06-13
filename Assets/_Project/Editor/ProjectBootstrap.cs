using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GuardiaoDosCristais;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GuardiaoDosCristaisEditor
{
    /// <summary>
    /// Gera o projeto completo com foco em fases maiores, composição modular
    /// de cenário e sprites consistentes com o pack do Kenney.
    /// </summary>
    public static class ProjectBootstrap
    {
        private const string Root = "Assets/_Project";
        private const string ScenePath = Root + "/Scenes";
        private const string SpritePath = Root + "/Sprites";
        private const string ResourceSpritePath = Root + "/Resources/Sprites";
        private const string KenneySpritePath = Root + "/ExternalAssets/Extracted/KenneyNewPlatformerPack/Sprites";
        private const int GroundLayer = 6;
        private const float BaseGroundY = -2.2f;
        private const float KillZoneY = -7.75f;

        private static readonly Dictionary<string, Sprite> S = new();

        [MenuItem("Tools/Guardiao dos Cristais/Gerar projeto completo")]
        public static void BuildCompleteProject()
        {
            EnsureTagsAndLayers();
            GenerateSprites();
            CreatePrefabs();

            CreateMenuScene();
            CreateGameOverScene();
            CreateVictoryScene();
            CreatePhase1();
            CreatePhase2();
            CreatePhase3();
            CreatePhase4();

            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GuardiaoDosCristais] Projeto gerado com arte modular e fases expandidas.");
        }

        // ═════════════════════════════════════════════════════════════════
        // TAGS & LAYERS
        // ═════════════════════════════════════════════════════════════════

        private static void EnsureTagsAndLayers()
        {
            var tagMgr = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            var tags = tagMgr.FindProperty("tags");
            AddTag(tags, "Climbable");

            var layers = tagMgr.FindProperty("layers");
            layers.GetArrayElementAtIndex(GroundLayer).stringValue = "Ground";

            tagMgr.ApplyModifiedProperties();
        }

        private static void AddTag(SerializedProperty tags, string tag)
        {
            for (int i = 0; i < tags.arraySize; i++)
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        }

        // ═════════════════════════════════════════════════════════════════
        // SPRITES
        // ═════════════════════════════════════════════════════════════════

        private static void GenerateSprites()
        {
            SyncKenneyGameplayArt();
            AssetDatabase.Refresh();
            LoadSprites();
        }

        private static void SyncKenneyGameplayArt()
        {
            string[] grassBlocks =
            {
                "terrain_grass_block_top_left",
                "terrain_grass_block_top",
                "terrain_grass_block_top_right",
                "terrain_grass_block_left",
                "terrain_grass_block_center",
                "terrain_grass_block_right",
                "terrain_grass_block_bottom_left",
                "terrain_grass_block_bottom",
                "terrain_grass_block_bottom_right",
            };

            string[] stoneBlocks =
            {
                "terrain_stone_block_top_left",
                "terrain_stone_block_top",
                "terrain_stone_block_top_right",
                "terrain_stone_block_left",
                "terrain_stone_block_center",
                "terrain_stone_block_right",
                "terrain_stone_block_bottom_left",
                "terrain_stone_block_bottom",
                "terrain_stone_block_bottom_right",
            };

            string[] backgrounds =
            {
                "background_color_trees",
                "background_fade_trees",
                "background_color_hills",
                "background_fade_hills",
                "background_color_mushrooms",
                "background_fade_mushrooms",
                "background_color_desert",
                "background_fade_desert",
                "background_solid_sky",
            };

            string[] props =
            {
                "bush",
                "rock",
                "fence",
                "mushroom_brown",
                "mushroom_red",
                "torch_on_a",
                "torch_on_b",
                "sign_left",
                "sign_right",
                "sign_exit",
            };

            CopySpriteGroup("Tiles/Default", "Tilesets", grassBlocks);
            CopySpriteGroup("Tiles/Default", "Tilesets", stoneBlocks);
            CopySpriteGroup("Backgrounds/Default", "Backgrounds", backgrounds);
            CopySpriteGroup("Tiles/Default", "Props", props);

            CopyKenneySprite("Tiles/Default/bridge_logs.png", "Tilesets/bridge_logs.png");
            CopyKenneySprite("Tiles/Default/spikes.png", "Tilesets/spikes.png");
            CopyKenneySprite("Tiles/Default/door_open.png", "Tilesets/portal.png", true);
            CopyKenneySprite("Tiles/Default/door_open_top.png", "Tilesets/door_open_top.png");
            CopyKenneySprite("Tiles/Default/ladder_bottom.png", "Tilesets/ladder_bottom.png");
            CopyKenneySprite("Tiles/Default/ladder_middle.png", "Tilesets/ladder_middle.png");
            CopyKenneySprite("Tiles/Default/ladder_top.png", "Tilesets/ladder_top.png");

            CopyKenneySprite("Characters/Default/character_beige_idle.png", "Player/kael_player.png");
            CopyKenneySprite("Characters/Default/character_beige_idle.png", "Player/p1_stand.png", true);
            CopyKenneySprite("Characters/Default/character_beige_walk_a.png", "Player/Walk/p1_walk01.png", true);
            CopyKenneySprite("Characters/Default/character_beige_walk_b.png", "Player/Walk/p1_walk02.png", true);
            CopyKenneySprite("Characters/Default/character_beige_walk_a.png", "Player/Walk/p1_walk03.png", true);
            CopyKenneySprite("Characters/Default/character_beige_walk_b.png", "Player/Walk/p1_walk04.png", true);
            CopyKenneySprite("Characters/Default/character_beige_jump.png", "Player/p1_jump.png", true);
            CopyKenneySprite("Characters/Default/character_beige_hit.png", "Player/p1_hurt.png", true);
            CopyKenneySprite("Characters/Default/character_beige_climb_a.png", "Player/p1_climb_a.png", true);
            CopyKenneySprite("Characters/Default/character_beige_climb_b.png", "Player/p1_climb_b.png", true);

            CopyKenneySprite("Enemies/Default/slime_normal_rest.png", "Enemies/slime.png", true);
            CopyKenneySprite("Enemies/Default/slime_normal_walk_a.png", "Enemies/slime_walk_a.png", true);
            CopyKenneySprite("Enemies/Default/slime_normal_walk_b.png", "Enemies/slime_walk_b.png", true);
            CopyKenneySprite("Enemies/Default/slime_fire_rest.png", "Boss/corrupted_guardian.png");

            CopyKenneySprite("Tiles/Default/gem_blue.png", "Collectibles/cristal_azul.png");
            CopyKenneySprite("Tiles/Default/gem_yellow.png", "Collectibles/cristal_dourado.png");
        }

        private static void CopySpriteGroup(string sourceFolder, string destinationFolder, IEnumerable<string> fileNames)
        {
            foreach (string fileName in fileNames)
                CopyKenneySprite($"{sourceFolder}/{fileName}.png", $"{destinationFolder}/{fileName}.png");
        }

        private static void CopyKenneySprite(string sourceRelativePath, string destinationRelativePath, bool copyToResources = false)
        {
            string sourceAssetPath = $"{KenneySpritePath}/{sourceRelativePath}";
            string destinationAssetPath = $"{SpritePath}/{destinationRelativePath}";

            CopyProjectAsset(sourceAssetPath, destinationAssetPath);

            if (copyToResources)
                CopyProjectAsset(sourceAssetPath, $"{ResourceSpritePath}/{destinationRelativePath}");
        }

        private static void CopyProjectAsset(string sourceAssetPath, string destinationAssetPath)
        {
            string sourceAbsolutePath = ToAbsoluteProjectPath(sourceAssetPath);
            string destinationAbsolutePath = ToAbsoluteProjectPath(destinationAssetPath);

            if (!File.Exists(sourceAbsolutePath))
            {
                Debug.LogWarning($"[GuardiaoDosCristais] Sprite ausente: {sourceAssetPath}");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationAbsolutePath)!);
            File.Copy(sourceAbsolutePath, destinationAbsolutePath, true);
            AssetDatabase.ImportAsset(destinationAssetPath, ImportAssetOptions.ForceUpdate);
        }

        private static string ToAbsoluteProjectPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static void LoadSprites()
        {
            S.Clear();

            LoadSpriteGroup("Tilesets", new[]
            {
                "terrain_grass_block_top_left",
                "terrain_grass_block_top",
                "terrain_grass_block_top_right",
                "terrain_grass_block_left",
                "terrain_grass_block_center",
                "terrain_grass_block_right",
                "terrain_grass_block_bottom_left",
                "terrain_grass_block_bottom",
                "terrain_grass_block_bottom_right",
                "terrain_stone_block_top_left",
                "terrain_stone_block_top",
                "terrain_stone_block_top_right",
                "terrain_stone_block_left",
                "terrain_stone_block_center",
                "terrain_stone_block_right",
                "terrain_stone_block_bottom_left",
                "terrain_stone_block_bottom",
                "terrain_stone_block_bottom_right",
                "bridge_logs",
                "spikes",
                "portal",
                "door_open_top",
                "ladder_bottom",
                "ladder_middle",
                "ladder_top",
            });

            LoadSpriteGroup("Backgrounds", new[]
            {
                "background_color_trees",
                "background_fade_trees",
                "background_color_hills",
                "background_fade_hills",
                "background_color_mushrooms",
                "background_fade_mushrooms",
                "background_color_desert",
                "background_fade_desert",
                "background_solid_sky",
            });

            LoadSpriteGroup("Props", new[]
            {
                "bush",
                "rock",
                "fence",
                "mushroom_brown",
                "mushroom_red",
                "torch_on_a",
                "torch_on_b",
                "sign_left",
                "sign_right",
                "sign_exit",
            });

            LoadSpriteAsset("kael_player", "Player/kael_player.png");
            LoadSpriteAsset("slime", "Enemies/slime.png");
            LoadSpriteAsset("corrupted_guardian", "Boss/corrupted_guardian.png");
            LoadSpriteAsset("cristal_azul", "Collectibles/cristal_azul.png");
            LoadSpriteAsset("cristal_dourado", "Collectibles/cristal_dourado.png");

            S["ground"] = S["terrain_grass_block_center"];
            S["platform"] = S["bridge_logs"];
            S["spike"] = S["spikes"];
            S["portal"] = S["portal"];
            S["climbable"] = S["ladder_middle"];
        }

        private static void LoadSpriteGroup(string folder, IEnumerable<string> names)
        {
            foreach (string name in names)
                LoadSpriteAsset(name, $"{folder}/{name}.png");
        }

        private static void LoadSpriteAsset(string key, string relativePath)
        {
            string path = $"{SpritePath}/{relativePath}";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[GuardiaoDosCristais] Falha ao carregar sprite: {path}");
                return;
            }

            S[key] = sprite;
        }

        // ═════════════════════════════════════════════════════════════════
        // PREFABS
        // ═════════════════════════════════════════════════════════════════

        private static void CreatePrefabs()
        {
            Save(BuildPlayer(Vector3.zero, false), $"{Root}/Prefabs/Player/Kael.prefab");
            Save(BuildEnemy(Vector3.zero, false), $"{Root}/Prefabs/Enemies/Slime.prefab");
            Save(BuildCrystal(Vector3.zero, false), $"{Root}/Prefabs/Collectibles/CristalAzul.prefab");
            Save(BuildMovPlat(new Vector3(0f, 0.8f, 0f), false), $"{Root}/Prefabs/Platforms/PlataformaMovel.prefab");
        }

        private static void Save(GameObject go, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        // ═════════════════════════════════════════════════════════════════
        // CENAS UI
        // ═════════════════════════════════════════════════════════════════

        private static void CreateMenuScene()
        {
            var scene = NewScene("MenuPrincipal");
            MakeCamera(Hex("D8F0F7"), null);
            MakeBackdropStrip("Menu_Fade", "background_fade_hills", -10f, 0.8f, 6, 2.8f, -30, new Color(0.74f, 0.86f, 0.96f));
            MakeBackdropStrip("Menu_Color", "background_color_trees", -10f, -0.2f, 6, 2.8f, -26, Color.white);
            MakeProp("kael_player", new Vector3(-5.8f, -0.9f, 0f), 1.15f, 8);
            MakeProp("portal", new Vector3(5.7f, -1.1f, 0f), 1.1f, 7, false, new Color(0.82f, 0.55f, 1f));
            MakeManagers();

            var canvas = MakeCanvas();
            var menu = new GameObject("MenuManager").AddComponent<MenuManager>();

            MakeText(canvas.transform, "Guardião dos Cristais", new Vector2(0f, 180f), 48, TextAnchor.MiddleCenter);
            MakeText(canvas.transform, "Recupere os cristais e atravesse fases maiores, mais densas e perigosas.", new Vector2(0f, 114f), 18, TextAnchor.MiddleCenter);

            LinkBtn(MakeButton(canvas.transform, "Jogar", new Vector2(0f, 34f)), menu.Play);
            LinkBtn(MakeButton(canvas.transform, "Controles", new Vector2(0f, -30f)), menu.ToggleControls);
            LinkBtn(MakeButton(canvas.transform, "Sair", new Vector2(0f, -94f)), menu.QuitGame);

            var panel = MakePanel(canvas.transform, "PainelControles", new Vector2(0f, -214f), new Vector2(760f, 128f));
            MakeText(panel.transform, "A/D: mover   Shift: correr   Espaço: pular   W/S: escalar   Esc: pausar", Vector2.zero, 17, TextAnchor.MiddleCenter);
            panel.SetActive(false);
            Set(menu, "controlsPanel", panel);

            SaveScene(scene, "MenuPrincipal");
        }

        private static void CreateGameOverScene()
        {
            var scene = NewScene("GameOver");
            MakeCamera(Hex("2C1C24"), null);
            MakeBackdropStrip("GameOver_Back", "background_fade_desert", -10f, 0.4f, 6, 3f, -25, new Color(0.68f, 0.58f, 0.68f));
            MakeManagers();

            var canvas = MakeCanvas();
            var manager = new GameObject("GameOverManager").AddComponent<GameOverManager>();
            Set(manager, "autoReturnToMenu", true);
            Set(manager, "returnDelay", 2.6f);

            MakeText(canvas.transform, "Game Over", new Vector2(0f, 138f), 54, TextAnchor.MiddleCenter);
            MakeText(canvas.transform, "Todas as vidas acabaram. Voltando ao menu inicial...", new Vector2(0f, 78f), 21, TextAnchor.MiddleCenter);

            LinkBtn(MakeButton(canvas.transform, "Tentar novamente", new Vector2(0f, 4f)), manager.TryAgain);
            LinkBtn(MakeButton(canvas.transform, "Voltar ao menu", new Vector2(0f, -60f)), manager.BackToMenu);

            SaveScene(scene, "GameOver");
        }

        private static void CreateVictoryScene()
        {
            var scene = NewScene("Vitoria");
            MakeCamera(Hex("D6F3E2"), null);
            MakeBackdropStrip("Victory_Fade", "background_fade_trees", -10f, 0.6f, 6, 3f, -28, new Color(0.82f, 0.91f, 0.98f));
            MakeBackdropStrip("Victory_Color", "background_color_trees", -10f, -0.2f, 6, 3f, -24, Color.white);
            MakeManagers();

            var canvas = MakeCanvas();
            var manager = new GameObject("VictoryManager").AddComponent<GameOverManager>();

            MakeText(canvas.transform, "Vitória!", new Vector2(0f, 138f), 52, TextAnchor.MiddleCenter);
            MakeText(canvas.transform, "Os cristais voltaram a brilhar e o reino está em paz.", new Vector2(0f, 80f), 22, TextAnchor.MiddleCenter);

            LinkBtn(MakeButton(canvas.transform, "Jogar novamente", new Vector2(0f, 4f)), manager.TryAgain);
            LinkBtn(MakeButton(canvas.transform, "Voltar ao menu", new Vector2(0f, -60f)), manager.BackToMenu);

            SaveScene(scene, "Vitoria");
        }

        // ═════════════════════════════════════════════════════════════════
        // FASES
        // ═════════════════════════════════════════════════════════════════

        private static void CreatePhase1()
        {
            var scene = NewScene("Fase01_Tutorial");
            var player = BuildPlayer(new Vector3(-8f, 0.4f, 0f), true);
            MakeCamera(new Color(0.97f, 0.97f, 0.98f, 1f), player.transform, -10f, 60f, -6f, 8.5f);
            MakeSkyBackdrop("Tutorial_Sky", -12f, 9, Hex("DDF7FF"));
            MakeBackdropStrip("Tutorial_Fade", "background_fade_hills", -12f, 1.1f, 9, 3f, -30, new Color(0.74f, 0.88f, 0.98f));
            MakeBackdropStrip("Tutorial_Color", "background_color_trees", -12f, -0.3f, 9, 3f, -24, Color.white);
            MakeManagers();
            MakeHUD("Fase 1 – Campo de Treinamento");

            MakeTerrainBlock("Tutorial_Ground", 24f, BaseGroundY, 68, 3, "grass");
            MakeBridgePlatform("Tut_Plataforma_1", -1.5f, 0.65f, 4);
            MakeBridgePlatform("Tut_Plataforma_2", 7.5f, 1.7f, 4);
            MakeBridgePlatform("Tut_Plataforma_3", 14.5f, 0.95f, 4);
            MakeBridgePlatform("Tut_Plataforma_4", 22.5f, 2.2f, 5);
            MakeBridgePlatform("Tut_Plataforma_5", 33f, 1.1f, 4);
            MakeBridgePlatform("Tut_Plataforma_6", 44f, 2.7f, 5);
            MakeClimbable(19f, BaseGroundY, 8);
            MakeBridgePlatform("Tut_LadderTop", 20f, 5.35f, 3);
            MakeSpikes(27f, BaseGroundY, 3);

            MakeProp("bush", new Vector3(-5f, BaseGroundY + 0.42f, 0f), 0.95f, 2);
            MakeProp("rock", new Vector3(2f, BaseGroundY + 0.36f, 0f), 0.9f, 2);
            MakeProp("fence", new Vector3(38f, BaseGroundY + 0.54f, 0f), 1f, 2);
            MakeProp("sign_right", new Vector3(-7f, BaseGroundY + 0.45f, 0f), 1f, 2);
            MakeProp("sign_exit", new Vector3(50f, BaseGroundY + 0.45f, 0f), 1f, 2);

            SpawnCrystals(Phase1Crystals());

            MakeWorldText("Use A e D\npara andar.", new Vector3(-4.8f, 1.65f, 0f));
            MakeWorldText("Espaço\npara pular.", new Vector3(6.8f, 2.95f, 0f));
            MakeWorldText("Shift para\ncorrer mais.", new Vector3(14.6f, 2.6f, 0f));
            MakeWorldText("W e S sobem\na escada.", new Vector3(20.2f, 5.95f, 0f));
            MakeWorldText("Evite os espinhos\ne alcance o portal.", new Vector3(38.5f, 1.35f, 0f));

            MakePortal(54f, BaseGroundY, false);
            MakeKillZone(-12f, 64f, -7f);

            SaveScene(scene, "Fase01_Tutorial");
        }

        private static (Vector3 pos, bool gold)[] Phase1Crystals() => new[]
        {
            (new Vector3(-7f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(-3f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3( 0f, 1.45f, 0f), false),
            (new Vector3( 4f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3( 7.5f, 2.45f, 0f), false),
            (new Vector3(11f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(14.5f, 1.75f, 0f), false),
            (new Vector3(18f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(22.5f, 3.0f, 0f), false),
            (new Vector3(27f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(33f, 1.95f, 0f), false),
            (new Vector3(38f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(44f, 3.45f, 0f), true),
            (new Vector3(48f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(51f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(19f, 5.9f, 0f), true),
        };

        private static void CreatePhase2()
        {
            var scene = NewScene("Fase02_Floresta");
            var player = BuildPlayer(new Vector3(-8f, 0.4f, 0f), true);
            MakeCamera(new Color(0.97f, 0.97f, 0.98f, 1f), player.transform, -10f, 86f, -6f, 9.5f);
            MakeSkyBackdrop("Forest_Sky", -12f, 11, Hex("DAF5E5"));
            MakeBackdropStrip("Forest_Fade", "background_fade_trees", -12f, 1.2f, 10, 3f, -30, new Color(0.77f, 0.88f, 0.96f));
            MakeBackdropStrip("Forest_Color", "background_color_trees", -12f, -0.15f, 10, 3f, -24, Color.white);
            MakeManagers();
            MakeHUD("Fase 2 – Floresta dos Cristais");

            MakeTerrainBlock("Forest_Ground_A", -2f, BaseGroundY, 16, 3, "grass");
            MakeTerrainBlock("Forest_Ground_B", 17f, BaseGroundY, 10, 3, "grass");
            MakeTerrainBlock("Forest_Ground_C", 34f, BaseGroundY, 12, 3, "grass");
            MakeTerrainBlock("Forest_Ground_D", 51f, BaseGroundY, 10, 3, "grass");
            MakeTerrainBlock("Forest_Ground_E", 68f, BaseGroundY, 12, 3, "grass");
            MakeTerrainBlock("Forest_Ground_F", 80f, BaseGroundY, 8, 3, "grass");

            MakeBridgePlatform("Forest_P1", 9f, -0.05f, 4);
            MakeBridgePlatform("Forest_P2", 19f, 1.65f, 4);
            MakeBridgePlatform("Forest_P3", 29f, 0.95f, 3);
            MakeBridgePlatform("Forest_P4", 36.5f, 2.65f, 5);
            MakeBridgePlatform("Forest_P5", 45f, 1.15f, 4);
            MakeBridgePlatform("Forest_P6", 55f, 2.15f, 4);
            MakeBridgePlatform("Forest_P7", 64.5f, 0.75f, 5);
            MakeBridgePlatform("Forest_P8", 71f, 2.85f, 4);

            MakeSpikes(1.5f, BaseGroundY, 3);
            MakeSpikes(31f, BaseGroundY, 4);
            MakeSpikes(48.5f, BaseGroundY, 3);
            MakeSpikes(66f, BaseGroundY, 3);

            BuildEnemy(new Vector3(0f, BaseGroundY + 0.25f, 0f), true);
            BuildEnemy(new Vector3(19f, 1.35f, 0f), true);
            BuildEnemy(new Vector3(34f, BaseGroundY + 0.25f, 0f), true);
            BuildEnemy(new Vector3(55f, 1.85f, 0f), true);
            BuildEnemy(new Vector3(72f, BaseGroundY + 0.25f, 0f), true);

            MakeProp("bush", new Vector3(-6f, BaseGroundY + 0.42f, 0f), 1f, 2);
            MakeProp("mushroom_brown", new Vector3(15f, BaseGroundY + 0.42f, 0f), 0.9f, 2);
            MakeProp("bush", new Vector3(33f, BaseGroundY + 0.42f, 0f), 1.05f, 2, true);
            MakeProp("fence", new Vector3(53f, BaseGroundY + 0.52f, 0f), 1f, 2);
            MakeProp("rock", new Vector3(70f, BaseGroundY + 0.38f, 0f), 1f, 2);
            MakeProp("sign_exit", new Vector3(81.5f, BaseGroundY + 0.45f, 0f), 1f, 2);

            SpawnCrystals(Phase2Crystals());

            MakePortal(80.5f, BaseGroundY, false, true, "grass");
            MakeKillZone(-12f, 91f, -7f);

            SaveScene(scene, "Fase02_Floresta");
        }

        private static (Vector3 pos, bool gold)[] Phase2Crystals() => new[]
        {
            (new Vector3(-7f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(-2f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3( 9f, 0.85f, 0f), false),
            (new Vector3(13f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(19f, 2.45f, 0f), true),
            (new Vector3(22f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(29f, 1.85f, 0f), false),
            (new Vector3(33f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(36.5f, 3.45f, 0f), false),
            (new Vector3(41f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(45f, 1.95f, 0f), false),
            (new Vector3(50f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(55f, 2.95f, 0f), false),
            (new Vector3(60f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(64.5f, 1.55f, 0f), false),
            (new Vector3(69f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(71f, 3.6f, 0f), true),
            (new Vector3(75f, BaseGroundY + 0.8f, 0f), false),
        };

        private static void CreatePhase3()
        {
            var scene = NewScene("Fase03_Caverna");
            var player = BuildPlayer(new Vector3(-8f, 0.4f, 0f), true);
            MakeCamera(new Color(0.85f, 0.88f, 0.92f, 1f), player.transform, -10f, 94f, -7f, 10.5f);
            MakeSkyBackdrop("Cave_Sky", -12f, 11, new Color(0.45f, 0.53f, 0.72f));
            MakeBackdropStrip("Cave_Fade", "background_fade_mushrooms", -12f, 1.4f, 11, 3f, -30, new Color(0.58f, 0.67f, 0.84f));
            MakeBackdropStrip("Cave_Color", "background_color_hills", -12f, -0.2f, 11, 3f, -25, new Color(0.34f, 0.47f, 0.59f));
            MakeManagers();
            MakeHUD("Fase 3 – Cavernas Perdidas");

            MakeTerrainBlock("Cave_Ground_A", -3f, BaseGroundY, 14, 4, "stone");
            MakeTerrainBlock("Cave_Ground_B", 14f, BaseGroundY, 8, 4, "stone");
            MakeTerrainBlock("Cave_Ground_C", 29f, BaseGroundY, 10, 4, "stone");
            MakeTerrainBlock("Cave_Ground_D", 44f, BaseGroundY, 8, 4, "stone");
            MakeTerrainBlock("Cave_Ground_E", 59f, BaseGroundY, 10, 4, "stone");
            MakeTerrainBlock("Cave_Ground_F", 77f, BaseGroundY, 14, 4, "stone");
            MakeTerrainBlock("Cave_Ground_G", 89f, BaseGroundY, 8, 4, "stone");

            MakeBridgePlatform("Cave_P1", 3f, 0.55f, 4);
            MakeBridgePlatform("Cave_P2", 11f, 2.2f, 3);
            MakeBridgePlatform("Cave_P3", 18f, 4.05f, 3);
            MakeBridgePlatform("Cave_P4", 27f, 1.05f, 4);
            MakeBridgePlatform("Cave_P5", 33f, 3.0f, 4);
            MakeBridgePlatform("Cave_P6", 42f, 1.45f, 3);
            MakeBridgePlatform("Cave_P7", 49f, 3.45f, 4);
            MakeBridgePlatform("Cave_P8", 58f, 1.15f, 4);
            MakeBridgePlatform("Cave_P9", 66f, 2.5f, 4);
            MakeBridgePlatform("Cave_P10", 76f, 3.2f, 5);

            BuildMovPlat(new Vector3(20f, 0.35f, 0f), true);
            BuildMovPlat(new Vector3(47f, 0.55f, 0f), true);
            BuildMovPlat(new Vector3(69f, 1.45f, 0f), true);

            MakeClimbable(14f, BaseGroundY, 8);
            MakeBridgePlatform("Cave_LadderTop_A", 15f, 5.35f, 3);
            MakeClimbable(61f, BaseGroundY, 7);
            MakeBridgePlatform("Cave_LadderTop_B", 62f, 4.35f, 3);

            MakeSpikes(-1f, BaseGroundY, 3);
            MakeSpikes(24f, BaseGroundY, 4);
            MakeSpikes(41f, BaseGroundY, 4);
            MakeSpikes(63f, BaseGroundY, 4);

            BuildEnemy(new Vector3(3f, 0.25f, 0f), true);
            BuildEnemy(new Vector3(11f, 1.95f, 0f), true);
            BuildEnemy(new Vector3(27f, 0.75f, 0f), true);
            BuildEnemy(new Vector3(33f, 2.7f, 0f), true);
            BuildEnemy(new Vector3(44f, BaseGroundY + 0.25f, 0f), true);
            BuildEnemy(new Vector3(49f, 3.15f, 0f), true);
            BuildEnemy(new Vector3(58f, 0.85f, 0f), true);
            BuildEnemy(new Vector3(76f, 2.9f, 0f), true);

            MakeProp("rock", new Vector3(-5f, BaseGroundY + 0.42f, 0f), 1f, 2);
            MakeProp("mushroom_red", new Vector3(12f, BaseGroundY + 0.44f, 0f), 0.95f, 2);
            MakeProp("torch_on_a", new Vector3(25f, 0.15f, 0f), 1f, 3);
            MakeProp("torch_on_b", new Vector3(52f, 0.8f, 0f), 1f, 3);
            MakeProp("rock", new Vector3(72f, BaseGroundY + 0.35f, 0f), 1f, 2, true);
            MakeProp("sign_exit", new Vector3(88f, BaseGroundY + 0.45f, 0f), 1f, 2);

            SpawnCrystals(Phase3Crystals());

            MakePortal(89.5f, BaseGroundY, false, true, "stone");
            MakeKillZone(-12f, 101f, -7f);

            SaveScene(scene, "Fase03_Caverna");
        }

        private static (Vector3 pos, bool gold)[] Phase3Crystals() => new[]
        {
            (new Vector3(-7f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(-2f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3( 3f, 1.55f, 0f), false),
            (new Vector3(11f, 2.95f, 0f), false),
            (new Vector3(14f, 4.9f, 0f), true),
            (new Vector3(18f, 4.8f, 0f), false),
            (new Vector3(23f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(27f, 1.85f, 0f), false),
            (new Vector3(33f, 3.8f, 0f), true),
            (new Vector3(37f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(42f, 2.2f, 0f), false),
            (new Vector3(47f, 1.3f, 0f), false),
            (new Vector3(49f, 4.2f, 0f), false),
            (new Vector3(54f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(58f, 1.9f, 0f), false),
            (new Vector3(61f, 4.2f, 0f), true),
            (new Vector3(66f, 3.25f, 0f), false),
            (new Vector3(69f, 2.35f, 0f), false),
            (new Vector3(73f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(76f, 4.0f, 0f), false),
            (new Vector3(82f, BaseGroundY + 0.8f, 0f), false),
        };

        private static void CreatePhase4()
        {
            var scene = NewScene("Fase04_TemploBoss");
            var player = BuildPlayer(new Vector3(-8f, 0.4f, 0f), true);
            MakeCamera(new Color(0.80f, 0.82f, 0.90f, 1f), player.transform, -10f, 100f, -7f, 11f);
            MakeSkyBackdrop("Temple_Sky", -12f, 12, new Color(0.43f, 0.47f, 0.70f));
            MakeBackdropStrip("Temple_Fade", "background_fade_desert", -12f, 1.2f, 11, 3f, -30, new Color(0.58f, 0.60f, 0.82f));
            MakeBackdropStrip("Temple_Color", "background_color_hills", -12f, -0.15f, 11, 3f, -25, new Color(0.38f, 0.42f, 0.58f));
            MakeManagers();
            MakeHUD("Fase 4 – Templo do Cristal Ancestral");

            MakeTerrainBlock("Temple_Ground_A", -3f, BaseGroundY, 14, 4, "stone");
            MakeTerrainBlock("Temple_Ground_B", 13.5f, BaseGroundY, 9, 4, "stone");
            MakeTerrainBlock("Temple_Ground_C", 28f, BaseGroundY, 10, 4, "stone");
            MakeTerrainBlock("Temple_Ground_D", 44f, BaseGroundY, 10, 4, "stone");
            MakeTerrainBlock("Temple_Ground_E", 60f, BaseGroundY, 12, 4, "stone");
            MakeTerrainBlock("Temple_Arena", 84f, BaseGroundY, 24, 4, "stone");

            MakeBridgePlatform("Temple_P1", 1f, 0.75f, 4);
            MakeBridgePlatform("Temple_P2", 8f, 2.45f, 3);
            MakeBridgePlatform("Temple_P3", 17f, 0.9f, 4);
            MakeBridgePlatform("Temple_P4", 24f, 2.85f, 3);
            MakeBridgePlatform("Temple_P5", 31f, 1.25f, 4);
            MakeBridgePlatform("Temple_P6", 39f, 3.2f, 3);
            MakeBridgePlatform("Temple_P7", 47f, 1.55f, 4);
            MakeBridgePlatform("Temple_P8", 55f, 3.1f, 4);
            MakeBridgePlatform("Temple_P9", 63f, 1.25f, 4);
            MakeBridgePlatform("Temple_P10", 73f, 2.25f, 4);
            MakeBridgePlatform("Temple_P11", 83f, 3.25f, 4);
            MakeBridgePlatform("Temple_P12", 91f, 2.25f, 4);

            BuildMovPlat(new Vector3(20f, 0.2f, 0f), true);
            BuildMovPlat(new Vector3(52f, 0.4f, 0f), true);
            MakeClimbable(6f, BaseGroundY, 8);
            MakeBridgePlatform("Temple_LadderTop", 7f, 5.35f, 3);

            MakeSpikes(0f, BaseGroundY, 4);
            MakeSpikes(26f, BaseGroundY, 4);
            MakeSpikes(43f, BaseGroundY, 4);
            MakeSpikes(68f, BaseGroundY, 3);

            BuildEnemy(new Vector3(1f, 0.45f, 0f), true);
            BuildEnemy(new Vector3(17f, 0.6f, 0f), true);
            BuildEnemy(new Vector3(31f, 0.95f, 0f), true);
            BuildEnemy(new Vector3(47f, 1.25f, 0f), true);
            BuildEnemy(new Vector3(63f, 0.95f, 0f), true);

            MakeProp("torch_on_a", new Vector3(10f, 0.8f, 0f), 1f, 3);
            MakeProp("torch_on_b", new Vector3(41f, 1.4f, 0f), 1f, 3);
            MakeProp("torch_on_a", new Vector3(71f, 0.8f, 0f), 1f, 3);
            MakeProp("fence", new Vector3(90f, BaseGroundY + 0.52f, 0f), 1f, 2);
            MakeProp("sign_exit", new Vector3(94f, BaseGroundY + 0.45f, 0f), 1f, 2);

            SpawnCrystals(Phase4Crystals());

            GameObject finalPortal = MakePortal(95f, BaseGroundY, true, true, "stone");
            BuildBoss(new Vector3(83f, BaseGroundY + 0.95f, 0f), finalPortal);
            MakeKillZone(-12f, 108f, -7f);

            SaveScene(scene, "Fase04_TemploBoss");
        }

        private static (Vector3 pos, bool gold)[] Phase4Crystals() => new[]
        {
            (new Vector3(-7f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(-2f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3( 1f, 1.75f, 0f), false),
            (new Vector3( 6f, 5.05f, 0f), true),
            (new Vector3( 8f, 3.25f, 0f), false),
            (new Vector3(13f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(17f, 1.85f, 0f), false),
            (new Vector3(24f, 3.6f, 0f), false),
            (new Vector3(28f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(39f, 3.95f, 0f), true),
            (new Vector3(47f, 2.25f, 0f), false),
            (new Vector3(55f, 3.95f, 0f), false),
            (new Vector3(60f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(63f, 1.95f, 0f), false),
            (new Vector3(73f, 3.15f, 0f), true),
            (new Vector3(83f, 4.1f, 0f), false),
            (new Vector3(88f, BaseGroundY + 0.8f, 0f), false),
            (new Vector3(91f, 3.15f, 0f), true),
            (new Vector3(94f, BaseGroundY + 0.8f, 0f), false),
        };

        // ═════════════════════════════════════════════════════════════════
        // BUILDERS
        // ═════════════════════════════════════════════════════════════════

        private static GameObject BuildPlayer(Vector3 pos, bool asSceneObj)
        {
            var go = new GameObject("Kael_Player");
            go.tag = "Player";
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = S.GetValueOrDefault("kael_player");
            sr.sortingOrder = 12;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.gravityScale = 2.2f;

            go.AddComponent<BoxCollider2D>();

            var groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(go.transform);
            groundCheck.localPosition = new Vector3(0f, -0.82f, 0f);

            var ctrl = go.AddComponent<PlayerController>();
            Set(ctrl, "groundCheck", groundCheck);
            Set(ctrl, "groundLayer", LayerMask.GetMask("Ground"));

            var health = go.AddComponent<PlayerHealth>();
            if (asSceneObj)
            {
                var respawn = new GameObject("RespawnPoint").transform;
                respawn.position = pos;
                Set(health, "respawnPoint", respawn);
            }

            go.AddComponent<PlayerAnimation>();
            return go;
        }

        private static GameObject BuildEnemy(Vector3 pos, bool asSceneObj)
        {
            var go = new GameObject("Slime");
            go.transform.position = pos;
            go.layer = 0;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = S.GetValueOrDefault("slime");
            sr.sortingOrder = 8;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 2f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            go.AddComponent<BoxCollider2D>();

            var wallCheck = new GameObject("WallCheck").transform;
            wallCheck.SetParent(go.transform);
            wallCheck.localPosition = new Vector3(0.5f, 0f, 0f);

            var groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(go.transform);
            groundCheck.localPosition = new Vector3(0.5f, -0.4f, 0f);

            var patrol = go.AddComponent<EnemyPatrol>();
            Set(patrol, "wallCheck", wallCheck);
            Set(patrol, "groundCheck", groundCheck);
            Set(patrol, "groundLayer", LayerMask.GetMask("Ground"));

            return go;
        }

        private static GameObject BuildBoss(Vector3 pos, GameObject finalPortal)
        {
            var go = new GameObject("Guardiao_Corrompido");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(1.7f, 1.7f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = S.GetValueOrDefault("corrupted_guardian");
            sr.sortingOrder = 9;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 2f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            go.AddComponent<BoxCollider2D>();

            var left = new GameObject("BossLimEsquerdo").transform;
            var right = new GameObject("BossLimDireito").transform;
            left.SetParent(go.transform, false);
            right.SetParent(go.transform, false);
            left.localPosition = new Vector3(-8f, 0f, 0f);
            right.localPosition = new Vector3(8f, 0f, 0f);

            var boss = go.AddComponent<BossController>();
            Set(boss, "leftLimit", left);
            Set(boss, "rightLimit", right);
            Set(boss, "finalPortal", finalPortal);
            return go;
        }

        private static GameObject BuildCrystal(Vector3 pos, bool gold)
        {
            var go = new GameObject(gold ? "Cristal_Dourado" : "Cristal_Azul");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = gold ? S.GetValueOrDefault("cristal_dourado") : S.GetValueOrDefault("cristal_azul");
            sr.sortingOrder = 7;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            var collectible = go.AddComponent<Collectible>();
            if (gold) Set(collectible, "crystalType", Collectible.CrystalType.Gold);
            return go;
        }

        private static GameObject BuildMovPlat(Vector3 pos, bool asSceneObj)
        {
            var platform = MakeBridgePlatform("Plataforma_Movel", pos.x, pos.y, 3);

            var pointA = new GameObject("PontoA").transform;
            var pointB = new GameObject("PontoB").transform;
            pointA.position = new Vector3(pos.x - 2.5f, pos.y, pos.z);
            pointB.position = new Vector3(pos.x + 2.5f, pos.y, pos.z);

            var movingPlatform = platform.AddComponent<MovingPlatform>();
            Set(movingPlatform, "pointA", pointA);
            Set(movingPlatform, "pointB", pointB);

            return platform;
        }

        // ═════════════════════════════════════════════════════════════════
        // HELPERS DE CENA
        // ═════════════════════════════════════════════════════════════════

        private static void SpawnCrystals((Vector3 pos, bool gold)[] crystals)
        {
            foreach (var (pos, gold) in crystals)
                BuildCrystal(pos, gold);
        }

        private static GameObject MakeTerrainBlock(string name, float centerX, float surfaceY, int width, int height, string theme)
        {
            string prefix = theme == "stone" ? "terrain_stone_block" : "terrain_grass_block";

            var root = new GameObject(name);
            root.transform.position = new Vector3(centerX, surfaceY, 0f);
            root.layer = GroundLayer;

            var col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, height);
            col.offset = new Vector2(0f, -height * 0.5f);

            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    string key = GetBlockTileKey(prefix, width, height, column, row);
                    if (!S.TryGetValue(key, out Sprite sprite)) continue;

                    var tile = new GameObject($"{name}_Tile_{column}_{row}");
                    tile.transform.SetParent(root.transform, false);
                    tile.transform.localPosition = new Vector3(
                        -width * 0.5f + 0.5f + column,
                        -0.5f - row,
                        0f);

                    var sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sortingOrder = row == 0 ? 0 : -1;
                }
            }

            return root;
        }

        private static string GetBlockTileKey(string prefix, int width, int height, int column, int row)
        {
            bool isTop = row == 0;
            bool isBottom = row == height - 1;
            bool isLeft = column == 0;
            bool isRight = column == width - 1;

            if (isTop && isLeft) return $"{prefix}_top_left";
            if (isTop && isRight) return $"{prefix}_top_right";
            if (isBottom && isLeft) return $"{prefix}_bottom_left";
            if (isBottom && isRight) return $"{prefix}_bottom_right";
            if (isTop) return $"{prefix}_top";
            if (isBottom) return $"{prefix}_bottom";
            if (isLeft) return $"{prefix}_left";
            if (isRight) return $"{prefix}_right";
            return $"{prefix}_center";
        }

        private static GameObject MakeBridgePlatform(string name, float centerX, float surfaceY, int width)
        {
            var root = new GameObject(name);
            root.transform.position = new Vector3(centerX, surfaceY, 0f);
            root.layer = GroundLayer;

            var col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, 0.34f);
            col.offset = new Vector2(0f, -0.17f);

            for (int column = 0; column < width; column++)
            {
                var tile = new GameObject($"{name}_Bridge_{column}");
                tile.transform.SetParent(root.transform, false);
                tile.transform.localPosition = new Vector3(-width * 0.5f + 0.5f + column, -0.5f, 0f);

                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = S.GetValueOrDefault("bridge_logs");
                sr.sortingOrder = 3;
            }

            return root;
        }

        private static void MakeSpikes(float centerX, float surfaceY, int count, float yOverride = float.NaN)
        {
            float startX = centerX - (count - 1) * 0.5f;
            float spikeY = float.IsNaN(yOverride) ? surfaceY + 0.52f : yOverride;
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"Espinho_{i}");
                go.transform.position = new Vector3(startX + i, spikeY, 0f);
                go.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = S.GetValueOrDefault("spikes");
                sr.sortingOrder = 6;

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.7f, 0.4f);
                col.offset = new Vector2(0f, 0.16f);

                var damage = go.AddComponent<DamageObject>();
                Set(damage, "respawnPlayer", true);
            }
        }

        private static void MakeClimbable(float centerX, float bottomY, int segments)
        {
            var root = new GameObject("Escada_Escalavel");
            root.tag = "Climbable";
            root.transform.position = new Vector3(centerX, bottomY, 0f);

            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.2f, segments + 0.2f);
            col.offset = new Vector2(0f, (segments + 0.2f) * 0.5f);

            for (int i = 0; i < segments; i++)
            {
                string key = i == 0
                    ? "ladder_bottom"
                    : i == segments - 1
                        ? "ladder_top"
                        : "ladder_middle";

                var rung = new GameObject($"{root.name}_{i}");
                rung.transform.SetParent(root.transform, false);
                rung.transform.localPosition = new Vector3(0f, i + 0.5f, 0f);

                var sr = rung.AddComponent<SpriteRenderer>();
                sr.sprite = S.GetValueOrDefault(key);
                sr.sortingOrder = 1;
            }
        }

        private static GameObject MakePortal(float x, float surfaceY, bool isFinal, bool addSupportBase = false, string supportTheme = "grass")
        {
            if (addSupportBase)
                MakeTerrainBlock($"Portal_Base_{x:0}", x, surfaceY, 4, supportTheme == "stone" ? 4 : 3, supportTheme);

            var root = new GameObject(isFinal ? "Portal_Final" : "Portal");
            root.transform.position = new Vector3(x, surfaceY - 0.06f, 0f);
            root.transform.localScale = Vector3.one;

            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = S.GetValueOrDefault("portal");
            sr.sortingOrder = 4;
            sr.color = new Color(0.74f, 0.54f, 1f, 1f);

            if (S.TryGetValue("door_open_top", out Sprite topSprite))
            {
                var top = new GameObject("PortalTop");
                top.transform.SetParent(root.transform, false);
                top.transform.localPosition = new Vector3(0f, 1f, 0f);

                var topRenderer = top.AddComponent<SpriteRenderer>();
                topRenderer.sprite = topSprite;
                topRenderer.sortingOrder = 5;
                topRenderer.color = new Color(0.92f, 0.82f, 1f, 1f);
            }

            root.AddComponent<BoxCollider2D>();
            root.AddComponent<Portal>();

            if (isFinal)
                root.SetActive(false);

            return root;
        }

        private static void MakeKillZone(float xStart, float xEnd, float y = -7f)
        {
            var root = new GameObject("KillZone");
            root.transform.position = new Vector3((xStart + xEnd) * 0.5f, y, 0f);

            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(xEnd - xStart, 2f);

            var damage = root.AddComponent<DamageObject>();
            Set(damage, "respawnPlayer", true);
            Set(damage, "damage", 1);
            Set(damage, "damageCooldown", 0.1f);
        }

        private static void MakeBackdropStrip(string name, string spriteKey, float startCenterX, float y, int count, float scale, int sortingOrder, Color tint)
        {
            if (!S.TryGetValue(spriteKey, out Sprite sprite)) return;

            var root = new GameObject(name);
            float step = 4f * scale;

            for (int i = 0; i < count; i++)
            {
                var bg = new GameObject($"{name}_{i}");
                bg.transform.SetParent(root.transform, false);
                bg.transform.position = new Vector3(startCenterX + i * step, y, 0f);
                bg.transform.localScale = new Vector3(scale, scale, 1f);

                var sr = bg.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = sortingOrder;
                sr.color = tint;
            }
        }

        private static void MakeSkyBackdrop(string name, float startCenterX, int count, Color tint)
        {
            float width = count * 12.5f;
            float centerX = startCenterX + width * 0.5f - 6f;
            MakeBackground($"{name}_Solid", centerX, 5f, width + 24f, 24f, tint);
            MakeBackdropStrip(name, "background_solid_sky", startCenterX, 4.15f, count, 3.1f, -32, tint);
        }

        private static void MakeBackground(string name, float centerX, float centerY, float width, float height, Color tint)
        {
            if (!S.TryGetValue("background_solid_sky", out Sprite sprite)) return;

            var go = new GameObject(name);
            go.transform.position = new Vector3(centerX, centerY, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -100;
            sr.color = tint;

            Vector2 spriteSize = sprite.bounds.size;
            go.transform.localScale = new Vector3(
                width / Mathf.Max(spriteSize.x, 0.01f),
                height / Mathf.Max(spriteSize.y, 0.01f),
                1f);
        }

        private static GameObject MakeProp(string spriteKey, Vector3 pos, float scale = 1f, int sortingOrder = 2, bool flipX = false, Color? tint = null)
        {
            if (!S.TryGetValue(spriteKey, out Sprite sprite)) return null;

            var go = new GameObject(spriteKey);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(flipX ? -scale : scale, scale, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            sr.color = tint ?? Color.white;

            return go;
        }

        private static void MakeWorldText(string text, Vector3 pos)
        {
            var go = new GameObject("Placa_" + text.Substring(0, Mathf.Min(text.Length, 20)));
            go.transform.position = pos;

            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = 0.065f;
            mesh.fontSize = 60;
            mesh.lineSpacing = 0.9f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            mesh.fontStyle = FontStyle.Bold;

            if (go.TryGetComponent(out MeshRenderer renderer))
                renderer.sortingOrder = 20;

            if (S.TryGetValue("background_solid_sky", out Sprite bgSprite))
            {
                string[] lines = text.Split('\n');
                int longestLine = 0;
                for (int i = 0; i < lines.Length; i++)
                    longestLine = Mathf.Max(longestLine, lines[i].Length);

                var bg = new GameObject("TextBG");
                bg.transform.SetParent(go.transform, false);
                bg.transform.localPosition = new Vector3(0f, 0f, 0.1f);

                var bgSr = bg.AddComponent<SpriteRenderer>();
                bgSr.sprite = bgSprite;
                bgSr.color = new Color(1f, 1f, 1f, 0.78f);
                bgSr.sortingOrder = 19;

                Vector2 bgSize = bgSprite.bounds.size;
                bg.transform.localScale = new Vector3(
                    (Mathf.Max(longestLine, 10) * 0.11f) / Mathf.Max(bgSize.x, 0.01f),
                    ((lines.Length * 0.34f) + 0.16f) / Mathf.Max(bgSize.y, 0.01f),
                    1f);
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // MANAGERS / CÂMERA / HUD / CANVAS
        // ═════════════════════════════════════════════════════════════════

        private static void MakeManagers()
        {
            var go = new GameObject("Managers");
            go.AddComponent<GameManager>();
            go.AddComponent<LevelManager>();
            go.AddComponent<AudioManager>();
        }

        private static void MakeCamera(Color bg, Transform target, float minX = 0f, float maxX = 0f, float minY = 0f, float maxY = 0f)
        {
            var go = new GameObject("Main Camera");
            var cam = go.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = bg;
            go.transform.position = new Vector3(0f, 1f, -10f);

            if (target == null) return;

            var follow = go.AddComponent<CameraFollow>();
            Set(follow, "target", target);
            Set(follow, "minX", minX);
            Set(follow, "maxX", maxX);
            Set(follow, "minY", minY);
            Set(follow, "maxY", maxY);
        }

        private static void MakeHUD(string title)
        {
            var canvas = MakeCanvas();

            var bar = MakePanel(canvas.transform, "HUD", Vector2.zero, new Vector2(0f, 58f));
            bar.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f, 0.72f);

            var barRect = bar.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.anchoredPosition = Vector2.zero;
            barRect.offsetMin = new Vector2(0f, -58f);
            barRect.offsetMax = Vector2.zero;

            var border = new GameObject("HUDBorder");
            border.transform.SetParent(barRect, false);
            var borderRect = border.AddComponent<RectTransform>();
            borderRect.anchorMin = new Vector2(0f, 0f);
            borderRect.anchorMax = new Vector2(1f, 0f);
            borderRect.pivot = new Vector2(0.5f, 0f);
            borderRect.anchoredPosition = Vector2.zero;
            borderRect.sizeDelta = new Vector2(0f, 2f);
            var borderImage = border.AddComponent<Image>();
            borderImage.color = new Color(1f, 1f, 1f, 0.15f);

            var lives = MakeHUDText(barRect, "♥ ♥ ♥", new Vector2(16f, -29f), new Vector2(170f, 34f), TextAnchor.MiddleLeft);
            var crystals = MakeHUDText(barRect, "◆ 00/00", new Vector2(194f, -29f), new Vector2(190f, 34f), TextAnchor.MiddleLeft);
            var score = MakeHUDText(barRect, "PTS 0000", new Vector2(376f, -29f), new Vector2(190f, 34f), TextAnchor.MiddleLeft);
            var level = MakeHUDText(barRect, title, new Vector2(-16f, -29f), new Vector2(470f, 34f), TextAnchor.MiddleRight);

            var levelRect = level.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(1f, 1f);
            levelRect.anchorMax = new Vector2(1f, 1f);
            levelRect.pivot = new Vector2(1f, 0.5f);

            var hud = canvas.gameObject.AddComponent<HUDController>();
            Set(hud, "livesText", lives);
            Set(hud, "crystalsText", crystals);
            Set(hud, "scoreText", score);
            Set(hud, "levelNameText", level);
        }

        private static Text MakeHUDText(Transform parent, string text, Vector2 pos, Vector2 size, TextAnchor anchor)
        {
            var t = MakeText(parent, text, pos, 18, anchor);
            var r = t.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(0f, 1f);
            r.pivot = anchor == TextAnchor.MiddleRight ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f);
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 12;
            t.resizeTextMaxSize = 18;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }

        private static Canvas MakeCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            go.AddComponent<GraphicRaycaster>();

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            return canvas;
        }

        private static Button MakeButton(Transform parent, string label, Vector2 pos)
        {
            var go = MakePanel(parent, "Botao_" + label, pos, new Vector2(270f, 52f));
            go.GetComponent<Image>().color = new Color(0.14f, 0.23f, 0.33f, 0.96f);

            var btn = go.AddComponent<Button>();
            var text = MakeText(go.transform, label, Vector2.zero, 20, TextAnchor.MiddleCenter);
            text.color = Color.white;
            return btn;
        }

        private static Text MakeText(Transform parent, string content, Vector2 pos, int size, TextAnchor anchor)
        {
            var go = new GameObject("Texto");
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(960f, 44f);

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = GetFont();
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            return text;
        }

        private static GameObject MakePanel(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            go.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.34f);
            return go;
        }

        private static void LinkBtn(Button btn, UnityEngine.Events.UnityAction action) =>
            UnityEventTools.AddPersistentListener(btn.onClick, action);

        private static Font GetFont() =>
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
            ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        // ═════════════════════════════════════════════════════════════════
        // UTILITÁRIOS
        // ═════════════════════════════════════════════════════════════════

        private static Scene NewScene(string name)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = name;
            return scene;
        }

        private static void SaveScene(Scene scene, string name) =>
            EditorSceneManager.SaveScene(scene, $"{ScenePath}/{name}.unity");

        private static void UpdateBuildSettings()
        {
            string[] scenes =
            {
                "MenuPrincipal",
                "Fase01_Tutorial",
                "Fase02_Floresta",
                "Fase03_Caverna",
                "Fase04_TemploBoss",
                "GameOver",
                "Vitoria",
            };

            var buildScenes = new EditorBuildSettingsScene[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
                buildScenes[i] = new EditorBuildSettingsScene($"{ScenePath}/{scenes[i]}.unity", true);

            EditorBuildSettings.scenes = buildScenes;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            return color;
        }

        private static void Set(object target, string field, object value)
        {
            FieldInfo fi = target.GetType().GetField(field,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (fi == null) return;

            if (fi.FieldType == typeof(LayerMask) && value is int mask)
            {
                fi.SetValue(target, new LayerMask { value = mask });
                return;
            }

            fi.SetValue(target, value);
        }
    }
}
