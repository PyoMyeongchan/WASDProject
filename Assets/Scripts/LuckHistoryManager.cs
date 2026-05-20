using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LuckHistoryManager
{
    private static readonly string SavePath =
        Path.Combine(Application.persistentDataPath, "luck_history.json");

    private const int MaxRecords = 30; // 최대 30일치만 보관

    public static void AddRecord(string item, string number, string message)
    {
        LuckHistoryData data = Load();

        // 오늘 기록이 이미 있으면 덮어쓰기
        data.records.RemoveAll(r => r.date == System.DateTime.Now.ToString("yyyy-MM-dd"));
        data.records.Add(new LuckRecord(item, number, message));

        // 오래된 기록 제거
        if (data.records.Count > MaxRecords)
            data.records.RemoveRange(0, data.records.Count - MaxRecords);

        Save(data);
    }

    public static List<LuckRecord> GetAllRecords()
    {
        LuckHistoryData data = Load();
        // 최신 날짜 먼저
        data.records.Sort((a, b) => string.Compare(b.date, a.date));
        return data.records;
    }

    public static void ClearHistory()
    {
        Save(new LuckHistoryData());
        Debug.Log("[LuckHistoryManager] 히스토리 삭제 완료");
    }

    private static LuckHistoryData Load()
    {
        if (!File.Exists(SavePath))
            return new LuckHistoryData();

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<LuckHistoryData>(json) ?? new LuckHistoryData();
    }

    private static void Save(LuckHistoryData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);
    }
}
