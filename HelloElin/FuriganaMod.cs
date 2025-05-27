using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using Ganss.Text;
using HalfFullWidth;
using HarmonyLib;
using JetBrains.Annotations;
using MyNihongo.KanaConverter;
using NPOI.SS.UserModel;

namespace MyMod;

internal static class ModInfo
{
    internal const string Guid = "dk.elinplugins.helloelinmod";
    internal const string Name = "My Elin Mod";
    internal const string Version = "1.0";
}

public class FuriganaEntry
{
    public string ruby { get; set; }
    public string? rt { get; set; }
}

public class FuriganaDictionaryEntry
{
    public string text { get; set; }
    public string reading { get; set; }
    public List<FuriganaEntry> furigana { get; set; }
}


internal class Furigana
{
}


// [HarmonyPatch]
// public class SourceLangInterceptor
// {
//     public static MethodBase TargetMethod()
//     {
//         // Type LangGameRowType = typeof(SourceLang<>).MakeGenericType(
//         //     typeof(LangGame.Row), typeof(LangGeneral.Row), typeof(LangList.Row), typeof(LangNote.Row), typeof(LangTalk.Row), typeof(LangWord.Row));
//         // return AccessTools.Method(LangGameRowType, "Get", new[] { typeof(string) });
//         return AccessTools.Method(typeof(SourceLang<LangGame.Row>), "Get", new[] { typeof(string) });
//     }
//     
//     public static void Prefix(string id)
//     {
//         // HelloElinMod.Log($"[SourceLangInterceptor] [Get] [Prefix] : {id}");
//     }
//
//     public static void Postfix(ref string __result)
//     {
//         // if (__result == null)
//         // {
//         //     HelloElinMod.Log("[SourceLangInterceptor] [Get] [Postfix] : NULL");
//         //     return;
//         // }
//         // HelloElinMod.Log($"[SourceLangInterceptor] [Get] [Postfix] : {__result} / {__result.Length}");
//         // __result = HelloElinMod.Convert(__result);
//     }
// }
//
// [HarmonyPatch]
// public class SourceLang2Interceptor
// {
//     public static MethodBase TargetMethod()
//     {
//         // Type LangGameRowType = typeof(SourceLang<>).MakeGenericType(
//         //     typeof(LangGame.Row), typeof(LangGeneral.Row), typeof(LangList.Row), typeof(LangNote.Row), typeof(LangTalk.Row), typeof(LangWord.Row));
//         // return AccessTools.Method(LangGameRowType, "Get", new[] { typeof(string) });
//         return AccessTools.Method(typeof(SourceLang<LangGeneral.Row>), "Get", new[] { typeof(string) });
//     }
//     
//     public static void Prefix(string id)
//     {
//         // HelloElinMod.Log($"[SourceLangInterceptor] [Get] [Prefix] : {id}");
//     }
//
//     public static void Postfix(ref string __result)
//     {
//         // if (__result == null)
//         // {
//         //     HelloElinMod.Log("[SourceLangInterceptor] [Get] [Postfix] : NULL");
//         //     return;
//         // }
//         // HelloElinMod.Log($"[SourceLangInterceptor] [Get] [Postfix] : {__result} / {__result.Length}");
//         // __result = HelloElinMod.Convert(__result);
//     }
// }

[HarmonyPatch(typeof(LangGame), "SetRow")]
public class LangGameSetRowInterceptor
{
    public static void Prefix(LangGame.Row r)
    {
        FuriganaMod.Log($"[LangGameSetRowInterceptor] [SetRow] [Prefix] : {r.text_JP}");
    }
}


[HarmonyPatch(typeof(LangGeneral), "SetRow")]
public class LangGeneralSetRowInterceptor
{
    public static void Prefix(LangGeneral.Row r)
    {
        FuriganaMod.Log($"[LangGeneralSetRowInterceptor] [SetRow] [Prefix] : {r.text_JP}");
    }
}

[HarmonyPatch(typeof(DramaManager), "AddEvent", new[] { typeof (DramaEvent)})]
public class DramaEventAddEventInterceptor
{
    public static void Prefix(DramaEvent e)
    {
        FuriganaMod.Log($"[DramaEventAddEventInterceptor] [AddEvent] [Prefix] : {e.step}");
    }

    public static void Postfix(ref DramaEvent __result)
    {
        if (__result.sequence.firstTalk != null)
        {
            FuriganaMod.Log($"[DramaEventAddEventInterceptor] [AddEvent] [Postfix] :");
        }
        // else
        // {
        //     HelloElinMod.Log($"[DramaEventAddEventInterceptor] [AddEvent] [Postfix] : {__result}");
        // }
    }
}

[HarmonyPatch(typeof(DramaSequence), "Play", new[] {typeof (string)})]
public class DramaSequencePlayInterceptor
{
    public static void Prefix(string id)
    {
        FuriganaMod.Log($"[DramaSequencePlayInterceptor] [Play] [Prefix] : {id}");
    }
    public static void Postfix()
    {}
}

[HarmonyPatch(typeof(DramaSequence), "Play", new[] {typeof (int)})]
public class DramaSequencePlay2Interceptor
{
    public static void Prefix(int eventID = 0)
    {
        FuriganaMod.Log($"[DramaSequencePlayInterceptor] [Play] [Prefix] : {eventID}");
    }
    public static void Postfix()
    {}
}

[HarmonyPatch(typeof(GameLang), "ConvertDrama")]
public class GameLangConvertDramaInterceptor
{
    public static void Prefix(string text, Chara c = null)
    {
        FuriganaMod.Log($"[GameLangConvertDramaInterceptor] [ConvertDrama] [Prefix] : {text} / {c.Name}");
    }

    public static void Postfix(ref string __result)
    {
        string converted = FuriganaMod.Convert(__result);
        FuriganaMod.Log($"[GameLangConvertDramaInterceptor] [ConvertDrama] [Postfix] : {__result} -> {converted}");
        __result = converted;
    }
}
//
// [HarmonyPatch(typeof(DramaManager), "Play")]
// public class DramaManagerPlayInterceptor
// {
//     public static void Prefix(DramaSetup setup)
//     {
//         // HelloElinMod.Log($"[DramaManagerPlayInterceptor] [Play] [Prefix] : {setup.textData} / {setup.sheet} / {setup.book} / {setup.tag} / {setup.step}");
//     }
//
//     public static void Postfix(ref DramaSequence __result)
//     {
//         ExcelData excelData = DramaManager.dictCache[CorePath.DramaData + __result.setup.book + ".xlsx"];
//         Dictionary<string, ExcelData.Sheet> sheets = excelData.sheets;
//         // HelloElinMod.Log($"[DramaManagerPlayInterceptor] [Play] [Postfix] : {sheets.Keys.Join(e => e, ",")}");
//
//         foreach (string sheetKey in sheets.Keys)
//         {
//             for (int i = 0; i < sheets[sheetKey].list.Count; i++)
//             {
//                 // HelloElinMod.Log($"[DramaManagerPlayInterceptor] [Play] [Postfix] : {sheetKey} / text_JP / {sheets[sheetKey].list[i]["text_JP"]}");
//                 sheets[sheetKey].list[i]["text_JP"] = HelloElinMod.Convert(sheets[sheetKey].list[i]["text_JP"]);
//             }
//         }
//
//         excelData.sheets = sheets;
//         DramaManager.dictCache[CorePath.DramaData + __result.setup.book + ".xlsx"] = excelData;
//
//         // string converted = HelloElinMod.Convert(__result);
//         // HelloElinMod.Log($"[LangGetInterceptor] [Get] [Postfix] : {__result} / {converted}");
//         // __result = converted;
//     }
// }


[HarmonyPatch(typeof(DramaManager), "Show")]
public class DramaManagerShowInterceptor
{
    public static void Prefix()
    {
        FuriganaMod.Log($"[DramaManagerShowInterceptor] [Show] [Prefix]");
    }

    public static void Postfix()
    {
        // string converted = HelloElinMod.Convert(__result);
        // HelloElinMod.Log($"[LangGetInterceptor] [Get] [Postfix] : {__result} / {converted}");
        // __result = converted;
    }
}

[HarmonyPatch(typeof(ExcelData), "GetText")]
public class ExcelDataGetTextInterceptor
{
    public static void Prefix(string id, string topic = "text")
    {
        FuriganaMod.Log($"[ExcelDataGetTextInterceptor] [GetText] [Prefix] : {id} / {topic}");
    }

    public static void Postfix(ref string __result)
    {
        // string converted = HelloElinMod.Convert(__result);
        // HelloElinMod.Log($"[LangGetInterceptor] [Get] [Postfix] : {__result} / {converted}");
        // __result = converted;
    }
}


[HarmonyPatch(typeof(ExcelDataList), "GetRow")]
public class ExcelDataListGetRowInterceptor
{
    public static void Prefix(string id)
    {
        FuriganaMod.Log($"[ExcelDataListGetRowInterceptor] [GetRow] [Prefix] : {id}");
    }

    public static void Postfix(ref Dictionary<string, string> __result)
    {
        // string converted = HelloElinMod.Convert(__result);
        // HelloElinMod.Log($"[LangGetInterceptor] [Get] [Postfix] : {__result} / {converted}");
        // __result = converted;
    }
}


[HarmonyPatch(typeof(ExcelParser), "GetStr")]
public class ExcelParserGetStrInterceptor
{
    public static void Prefix(int id, bool useDefault = false)
    {
        FuriganaMod.Log($"[ExcelParserGetStrInterceptor] [GetStr] [Prefix] : {id} / {useDefault}");
    }

    public static void Postfix(ref string __result)
    {
        // string converted = HelloElinMod.Convert(__result);
        // HelloElinMod.Log($"[LangGetInterceptor] [Get] [Postfix] : {__result} / {converted}");
        // __result = converted;
    }
}

[HarmonyPatch(typeof(ExcelParser), "GetStringArray")]
public class ExcelParserGetStringArrayInterceptor
{
    public static void Prefix(int id)
    {
        FuriganaMod.Log($"[ExcelParserGetStringArrayInterceptor] [GetStringArray] [Prefix] : {id}");
    }

    public static void Postfix(ref string[] __result)
    {
        // string converted = HelloElinMod.Convert(__result);
        // HelloElinMod.Log($"[LangGetInterceptor] [Get] [Postfix] : {__result} / {converted}");
        // __result = converted;
    }
}

[HarmonyPatch(typeof(Lang), "Get")]
public class LangGetInterceptor
{
    public static bool isUiText = false;
    public static void Prefix(string id)
    {
        if (Regex.Matches(id, @"^[0-9a-zA-Z_]*$").Count > 0)
            isUiText = true;
        else
            isUiText = false;
        
        // HelloElinMod.Log($"[LangGetInterceptor] [Get] [Prefix] : {id} / {isUiText}");
    }

    public static void Postfix(ref string __result)
    {
        if (__result == null)
        {
            // HelloElinMod.Log("[LangGetInterceptor] [Get] [Postfix] : NULL");
            return;
        } else if (isUiText)
        {
            // HelloElinMod.Log("[LangGetInterceptor] [Get] [Postfix] : UI text");
            return;
        }
        string converted = FuriganaMod.Convert(__result);
        // if (__result.Contains("冒険者のために"))
            FuriganaMod.Log($"[LangGetInterceptor] [Get] [Postfix] : {__result} / {converted}");
        __result = converted;
    }
}

[HarmonyPatch(typeof(Scene), "Init")]
public class SceneInitInterceptor
{
    public static void Prefix(Scene.Mode newMode)
    {
        FuriganaMod.Log($"[SceneInitInterceptor] [Init] [Prefix] : {newMode}");
    }
}
    [HarmonyPatch(typeof(global::IO), "LoadText")]
    public class IOLoadTextInterceptor
    {
        public static void Prefix(string _path)
        {
            // HelloElinMod.Log("[IO] [LoadText] [Prefix] : " + _path);
        }
    
        public static void Postfix(ref string __result)
        {
            __result = FuriganaMod.Convert(__result);
        }
    }
    
    [HarmonyPatch(typeof(global::IO), "LoadTextArray")]
    public class IOLoadTextArrayInterceptor
    {
        public static void Prefix(string _path)
        {
            // HelloElinMod.Log("[IO] [LoadTextArray] [Prefix] : " + _path);
        }
    
        public static void Postfix(ref string[] __result)
        {
            for (int i = 0; i < __result.Length; i++)
            {
                __result[i] = FuriganaMod.Convert(__result[i]);
            }
        }
    }


    [BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
    internal class FuriganaMod : BaseUnityPlugin
    {
        [CanBeNull] internal static FuriganaMod Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            furiganaDictionary = LoadDictionary();
            Log($"[Furigana] [Furigana] Json loaded {furiganaDictionary.Count}");
            furiganaHashmap = BuildHashMap(furiganaDictionary);
            InitSearchEngine();
            Harmony.DEBUG = true;
            var harmony = new Harmony(ModInfo.Guid);
            harmony.PatchAll();
            string patchResult = string.Join(", ", harmony.GetPatchedMethods().Select(e => $"{e.Name} | {e.Module} | {e.DeclaringType}").ToList());
            Log($"[Furigana] [Furigana] Patch: {patchResult}");
            
            for (int i = 0; i < Lang.General.rows.Count; i++)
            {
                Lang.General.rows[i].text_JP = FuriganaMod.Convert(Lang.General.rows[i].text_JP);
            }
            
            for (int i = 0; i < Lang.Game.rows.Count; i++)
            {
                Lang.Game.rows[i].text_JP = FuriganaMod.Convert(Lang.Game.rows[i].text_JP);
            }
            
            for (int i = 0; i < Lang.Note.rows.Count; i++)
            {
                Lang.Note.rows[i].text_JP = FuriganaMod.Convert(Lang.Note.rows[i].text_JP);
            }
            Log($"[SceneInitInterceptor] [Init] [Prefix] : {Lang.General.rows.Count} / {Lang.General.rows[0].text_JP}");
        }

        internal static void Log(string msg)
        {
            if (!DEBUG)
                return;
            Instance!.Logger.LogInfo(msg);
        }


        private const bool DEBUG = false;
        private const string SPLITTER_FRONT = "「";
        private const string SPLITTER_BACK = "」";
        private const string PREFIX = "";
        public static List<FuriganaDictionaryEntry> furiganaDictionary;
        public static Dictionary<string, FuriganaDictionaryEntry> furiganaHashmap;
        public static Trie trie;
        public static AhoCorasick ahoCorasick;

        internal void InitSearchEngine()
        {
            ahoCorasick = new AhoCorasick(furiganaDictionary.Select(e => e.text).ToList());
        }

    internal List<FuriganaDictionaryEntry> LoadDictionary()
    {
        var assembly = Assembly.GetExecutingAssembly();
        Log("[Furigana] [LoadDictionary] Assembly");
        foreach (string res in assembly.GetManifestResourceNames())
        {
            if (res != null) 
                Log($"[Furigana] [LoadDictionary] Resource : {res}");
        }
        
        using Stream? stream = assembly.GetManifestResourceStream("HelloElin.JmdictFurigana.json");
        if (stream == null)
            throw new Exception($"[Furigana] [LoadDictionary] Dictionary not found");
        
        using StreamReader reader = new StreamReader(stream);
        // return reader.ReadToEnd();
        return IO.LoadJSON<List<FuriganaDictionaryEntry>>(reader.ReadToEnd()).OrderByDescending(e => e.text.Length).ToList();
        // return "[]";
    }

    internal Dictionary<string, FuriganaDictionaryEntry> BuildHashMap(List<FuriganaDictionaryEntry> furiganaDictionary)
    {
        return furiganaDictionary.GroupBy(e => e.text).Select(e => e.First()).ToDictionary(e => e.text);
    }
    
    internal static bool ContainsHankakuKatakana(string input)
    {
        foreach (char c in input)
        {
            if (c >= '\uFF61' && c <= '\uFF9F')
            {
                return true;
            }
        }
        return false;
    }

    internal static string Convert(string input)
    {
        // if (input.Contains("冒険者のために"))
        //     Log($"[Furigana] [Convert] {input}");
        // Log($"[Furigana] [Convert] {input.Length == 0} / {Regex.Matches(input, @"\{[a-zA-Z]+\,").Count > 0} / {input.Contains(PREFIX)}");
        if (input.Length == 0
            || Regex.Matches(input, @"\{[a-zA-Z]+[,|]+").Count > 0
            // || input.Contains(PREFIX)
            || ContainsHankakuKatakana(input)
            || Regex.Matches(input, @"^#[0-9]*").Count > 0)
        {
            // Log($"[Furigana] [Convert] skipped: {input}");
            return input;
        }

        List<WordMatch> ahoCorasickResultList = ahoCorasick.Search(input).ToList();
        // Log($"[Furigana] [Convert] : {input}");
        // Log($"[Furigana] [Convert] : {string.Join("", ahoCorasickResultList.Select(e => $"{e.Word}[{e.Index}]"))}");

        int strPoint = 0;
        string convertedInput = $"{PREFIX}";
        while (strPoint < input.Length)
        {
            WordMatch? bestMatch = ahoCorasickResultList.Where(e => e.Index == strPoint).OrderByDescending(e => e.Word.Length).Cast<WordMatch?>().FirstOrDefault();
            if (bestMatch.HasValue)
            {
                convertedInput += bestMatch.Value.Word; // $"{bestMatch.Value.Word}[{bestMatch.Value.Index}]";
                string convertedWord = "";
                foreach (FuriganaEntry furigana in furiganaHashmap[bestMatch.Value.Word].furigana)
                {
                    convertedWord += $"{furigana.rt ?? furigana.ruby}";
                }
                convertedInput += $"{SPLITTER_FRONT}{convertedWord.KanaToKatakana(UnrecognisedCharacterPolicy.Append).ToHalfwidthString()}{SPLITTER_BACK}";
                strPoint += bestMatch.Value.Word.Length;
            }
            else
            {
                convertedInput += input[strPoint];
                strPoint++;
            }
        }

        // convertedInput += $"{PREFIX}";
        
        // Log($"[Furigana] [Convert] : {convertedInput}");
        return convertedInput;
    }
}
