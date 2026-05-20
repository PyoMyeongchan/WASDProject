using System;
using UnityEngine;

public static class SaveManager
{
    private const string KEY_LAST_DATE    = "LuckSlot_LastDate";
    private const string KEY_LUCKY_ITEM   = "LuckSlot_Item";
    private const string KEY_LUCKY_NUMBER = "LuckSlot_Number";
    private const string KEY_LUCKY_MSG    = "LuckSlot_Message";

    public static bool HasPlayedToday()
    {
        string saved = PlayerPrefs.GetString(KEY_LAST_DATE, string.Empty);
        return saved == GetTodayString();
    }

    public static void SaveResult(string item, string number, string message)
    {
        PlayerPrefs.SetString(KEY_LAST_DATE,    GetTodayString());
        PlayerPrefs.SetString(KEY_LUCKY_ITEM,   item);
        PlayerPrefs.SetString(KEY_LUCKY_NUMBER, number);
        PlayerPrefs.SetString(KEY_LUCKY_MSG,    message);
        PlayerPrefs.Save();
    }

    public static (string item, string number, string message) LoadResult()
    {
        return (
            PlayerPrefs.GetString(KEY_LUCKY_ITEM,   string.Empty),
            PlayerPrefs.GetString(KEY_LUCKY_NUMBER, string.Empty),
            PlayerPrefs.GetString(KEY_LUCKY_MSG,    string.Empty)
        );
    }

    public static void ClearSaveData()
    {
        PlayerPrefs.DeleteKey(KEY_LAST_DATE);
        PlayerPrefs.DeleteKey(KEY_LUCKY_ITEM);
        PlayerPrefs.DeleteKey(KEY_LUCKY_NUMBER);
        PlayerPrefs.DeleteKey(KEY_LUCKY_MSG);
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] 저장 데이터 초기화 완료");
    }

    private static string GetTodayString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }
}
