using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LuckData", menuName = "LuckSlot/LuckData")]
public class LuckDataSO : ScriptableObject
{
    [Header("행운 물건 이름 (luckyItemIcons 와 같은 인덱스 사용)")]
    public string[] luckyItems = new string[]
    {
        "네잎클로버", "별", "달", "무지개", "다이아몬드",
        "열쇠", "벚꽃", "나침반", "반짝이", "사탕"
    };

    [Header("행운 물건 아이콘 (luckyItems 와 인덱스 일치)")]
    public Sprite[] luckyItemIcons;

    [Header("행운 숫자")]
    public string[] luckyNumbers = new string[]
    {
        "7", "3", "13", "21", "42",
        "8", "11", "17", "28", "99"
    };

    [Header("행운 문구")]
    public string[] luckyMessages = new string[]
    {
        "오늘은 좋은 일이 생길 거예요!",
        "당신의 미소가 행운을 부릅니다.",
        "작은 행동이 큰 변화를 만들어요.",
        "오늘 하루도 빛나는 하루가 될 거예요!",
        "행운은 준비된 자에게 찾아옵니다.",
        "오늘은 무엇이든 잘 될 것 같은 날이에요.",
        "당신 주변에 행운이 가득합니다.",
        "오늘의 선택이 내일의 행복이 됩니다.",
        "긍정적인 마음이 행운을 끌어당깁니다.",
        "오늘 하루, 당신은 특별한 사람입니다!"
    };

    public Sprite GetItemIcon(string itemName)
    {
        if (luckyItemIcons == null || luckyItemIcons.Length == 0) return null;
        int idx = Array.IndexOf(luckyItems, itemName);
        if (idx >= 0 && idx < luckyItemIcons.Length)
            return luckyItemIcons[idx];
        return null;
    }
}
