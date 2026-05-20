using System;
using System.Collections.Generic;

[Serializable]
public class LuckRecord
{
    public string date;      // "yyyy-MM-dd"
    public string item;
    public string number;
    public string message;

    public LuckRecord(string item, string number, string message)
    {
        this.date    = DateTime.Now.ToString("yyyy-MM-dd");
        this.item    = item;
        this.number  = number;
        this.message = message;
    }

    public string DisplayDate => DateTime.Parse(date).ToString("yyyy년 MM월 dd일");
}

[Serializable]
public class LuckHistoryData
{
    public List<LuckRecord> records = new List<LuckRecord>();
}
