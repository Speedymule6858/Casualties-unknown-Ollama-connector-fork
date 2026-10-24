using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CasualtiesOllama
{
    /// <summary>Which "senses" the AI receives.</summary>
    public class SenseSettings
    {
        public bool Vitals = true;
        public bool Needs = true;
        public bool StatusIcons = true;   // the icons at the bottom of the screen, as readable text
        public bool Limbs = true;
        public bool Collision = true;
        public bool Map = true;
        public bool Items = true;
        public bool Objects = true;       // world objects: crates, buttons, plants, trees (with health)
        public bool Players = true;       // other human players (multiplayer)
        public bool Creatures = true;
        public bool Hazards = true;       // traps, mines, saws...
        public bool Climbables = true;    // ropes / ladders
        public bool Inventory = true;
        public bool Wearing = true;
        public bool Craftables = true;    // only recipes that can be crafted right now
        public bool Events = true;
        public bool DamageLog = true;     // "what hit me"
        public bool Chat = true;          // multiplayer chat lines
        public bool Pointer = true;       // the spot the human pointed at
        public bool Position = false;
        public bool Screenshot = false;
        public float ScreenshotScale = 1.0f;
    }

    /// <summary>What the AI may DO. Forbidden actions are rejected and the AI is told.</summary>
    public class PermissionSettings
    {
        public bool Move = true;
        public bool Jump = true;
        public bool Crouch = true;
        public bool Aim = true;
        public bool Attack = true;
        public bool UseItems = true;
        public bool ApplyToLimbs = true;
        public bool Grab = true;
        public bool DropThrow = true;
        public bool Wear = true;
        public bool Inventory = true;
        public bool Craft = true;
        public bool Inspect = true;
        public bool Speak = true;
        public bool Interact = true;       // use world objects (buttons, crates, plants)
        public bool Storage = true;        // put items into / take items out of bags, combine items, batteries
        public bool Carry = true;          // piggyback / carry players (multiplayer)
        public bool Chat = true;
    }

    public class AiSettings
    {
        // ---- connection ----
        public string ApiKind = "ollama";      // ollama | openai   which API the server speaks
        public string OllamaUrl = "http://localhost:11434";  // Ollama base URL, OR the base URL of an OpenAI-compatible server
            public string ApiKey = "";             // optional API key (cloud / proxy, never saved in profiles)
            public string Model = "";
            public float Temperature = 0.4f;
            public int NumCtx = 8192;
            public int NumPredict = 700;
            public string Think = "default";       // default | off | on
            public string KeepAlive = "30m";
            public int TimeoutSec = 180;

            // ---- identity ----
            public string AiName = "Bot";
            public string AiAliases = "";          // comma separated extra names it answers to

            // ---- agent behaviour ----
            public int MaxActionsPerTurn = 4;
            public float MinThinkInterval = 0.3f;
            public bool PauseWhileThinking = true;
            public bool ForceNormalSpeed = true;
            public bool BlockHumanInput = true;
            public bool AutoJumpHelper = true;
            public bool LedgeGuard = true;         // refuse to walk off big drops unless it uses 'leap'
            public bool InterruptOnChat = true;
            public bool AutoFollowPointer = true;  // follow the pointer without calling the LLM (saves credits)
            public bool LogToFile = true;
            public bool AutoCrouch = true;        // crouch automatically when a 1-block-high tunnel is ahead
            public bool AutoGrabRope = true;       // grab a rope the moment it is within reach (also while falling past them)
            public bool KeepShortMemory = true;    // short-term memory + journal are saved to disk and survive restarts and new lives
            public string MapMode = "compact";     // compact | ascii | both

            // ---- rate limits (0 = unlimited) ----
            public int MaxCallsPerMinute = 0;
            public int MaxCallsPerHour = 0;
            public int MaxTokensPerHour = 0;
            public float MinSecondsBetweenCalls = 0f;
            public bool StopWhenBudgetHit = false;
            public bool PauseGameWhileRateLimited = false;

            // ---- memory ----
            public bool ShortMemory = true;        // recent turns + journal
            public bool LongMemory = true;         // lessons / post-mortems
            public int HistoryTurns = 6;
            public int SummarizeEvery = 6;
            public int MaxLessonsInPrompt = 15;
            public string MemoryProfile = "default";
            public string ActiveProfile = "";

            // ---- senses tuning ----
            public int MapHalfWidth = 12;
            public int MapHalfHeight = 6;
            public float ItemRadius = 14f;
            public float CreatureRadius = 18f;
            public bool ShowItemFlags = false;

            // ---- multiplayer ----
            public bool ReadChat = true;
            public string ChatMode = "mentions";   // never | mentions | all   (when to react immediately)
            public bool ReplyInChat = true;
            public int ChatLines = 8;
            public float ChatCooldown = 3f;

            // ---- text ----
            public string Mission = "Survive as long as possible. Stay alive first (stop bleeding, eat and drink), avoid traps and dangerous creatures, and explore carefully to find useful items.";
            public string SystemPromptOverride = "";

            // ---- hotkeys ----
            public string GuiKey = "F8";
            public string ToggleKey = "F9";
            public string StopKey = "F10";
            public string PointerKey = "F7";

            public bool UsesOpenAi => string.Equals(ApiKind, "openai", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ApiKind, "openai-compatible", StringComparison.OrdinalIgnoreCase);

            public SenseSettings Sense = new SenseSettings();
            public PermissionSettings Allow = new PermissionSettings();

            [System.Text.NonSerialized] public string Path;

            public static AiSettings Load(string dir)
            {
                string path = System.IO.Path.Combine(dir, "settings.json");
                AiSettings s = null;
                try { if (System.IO.File.Exists(path)) s = Newtonsoft.Json.JsonConvert.DeserializeObject<AiSettings>(System.IO.File.ReadAllText(path)); }
                catch (Exception e) { Plugin.Log?.LogWarning("settings.json unreadable, using defaults: " + e.Message); }
                if (s == null) s = new AiSettings();
                if (s.Sense == null) s.Sense = new SenseSettings();
                if (s.Allow == null) s.Allow = new PermissionSettings();
                s.Path = path;
                return s;
            }

            public void Save()
            {
                try { System.IO.File.WriteAllText(Path, Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented)); }
                catch (Exception e) { Plugin.Log?.LogWarning("Could not save settings: " + e.Message); }
            }
    }
}
