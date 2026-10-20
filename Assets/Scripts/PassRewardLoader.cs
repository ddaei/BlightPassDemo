using System.Collections.Generic;
using System.Text;
using UnityEngine;

// 「집행관 정식 임명」 패스 보상 표(CSV)를 읽어서 화면에 보여주는 스크립트
// - CSV 위치: Assets/Resources/pass_rewards.csv
// - 엑셀에서 숫자를 고치고 CSV만 바꿔 넣으면, 코드를 고치지 않아도 화면이 바뀐다
public class PassRewardLoader : MonoBehaviour
{
    // ===== 인스펙터 창에서 바로 바꿀 수 있는 값 =====
    public string csvFileName = "pass_rewards";          // Resources 폴더 안 CSV 이름 (.csv는 빼고)
    public string title = "「집행관 정식 임명」 패스";      // 화면 맨 위 제목
    public int fontSize = 22;                            // 글자 크기
    public bool premiumOnly = false;                     // 체크하면 프리미엄 보상만 보여줌
    // ===== 보상 한 줄을 담는 상자 =====
    class Reward
    {
        public int level;      // 레벨
        public string track;   // 일반 / 프리미엄
        public string item;    // 아이템 이름
        public int amount;     // 수량
    }

    List<Reward> rewards = new List<Reward>();
    Vector2 scroll;

    // 게임이 시작되면 한 번 실행된다
    void Start()
    {
        LoadCsv();
    }

    // CSV 파일을 읽어서 rewards 목록에 담는다
    void LoadCsv()
    {
        TextAsset csv = Resources.Load<TextAsset>(csvFileName);
        if (csv == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없어요: Assets/Resources/" + csvFileName + ".csv");
            return;
        }

        string[] lines = csv.text.Replace("\r", "").Split('\n');

        // 맨 위 제목 줄들은 아래에서 '레벨이 숫자가 아니면 건너뛰기'로 걸러진다
        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            List<string> cells = SplitCsvLine(lines[i]);
            if (cells.Count < 8) continue;

            // 칸 순서: 0 reward_id, 1 레벨, 2 누적 EXP, 3 최소 일차, 4 트랙, 5 item_id, 6 아이템, 7 수량, 8 규칙
            Reward r = new Reward();
            r.level = ToInt(cells[1]);
            r.track = cells[4].Trim();
            r.item = cells[6].Trim();
            r.amount = ToInt(cells[7]);
            if (r.level <= 0) continue;   // 제목 줄처럼 레벨이 숫자가 아닌 줄은 건너뜀
            rewards.Add(r);
        }

        Debug.Log("보상 " + rewards.Count + "줄을 불러왔어요.");
    }

    // "1,000" 같은 글자를 숫자 1000으로 바꾼다
    int ToInt(string text)
    {
        int value;
        int.TryParse(text.Replace(",", "").Replace("\"", "").Trim(), out value);
        return value;
    }

    // CSV 한 줄을 칸별로 나눈다 ("1,000"처럼 따옴표 안의 쉼표는 나누지 않음)
    List<string> SplitCsvLine(string line)
    {
        List<string> cells = new List<string>();
        StringBuilder current = new StringBuilder();
        bool inQuotes = false;

        foreach (char ch in line)
        {
            if (ch == '"') inQuotes = !inQuotes;
            else if (ch == ',' && !inQuotes)
            {
                cells.Add(current.ToString());
                current.Length = 0;
            }
            else current.Append(ch);
        }
        cells.Add(current.ToString());
        return cells;
    }

    // 한 레벨, 한 트랙의 보상을 "아이템 ×수량" 글자로 만든다
    string RewardText(int level, string track)
    {
        StringBuilder sb = new StringBuilder();
        foreach (Reward r in rewards)
        {
            if (r.level != level || r.track != track) continue;
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(r.item + " ×" + r.amount.ToString("N0"));
        }
        return sb.Length == 0 ? "-" : sb.ToString();
    }

    // 매 프레임 화면에 글자를 그린다
    void OnGUI()
    {
        GUI.skin.label.fontSize = fontSize;
        GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));

        GUILayout.Label(title);
        GUILayout.Space(10);

        scroll = GUILayout.BeginScrollView(scroll);

        int maxLevel = 0;
        foreach (Reward r in rewards) if (r.level > maxLevel) maxLevel = r.level;

        for (int lv = 1; lv <= maxLevel; lv++)
        {
            if (premiumOnly)
            {
                GUILayout.Label("Lv." + lv + "    프리미엄: " + RewardText(lv, "프리미엄"));
            }
            else
            {
                GUILayout.Label("Lv." + lv
                    + "    일반: " + RewardText(lv, "일반")
                    + "    |    프리미엄: " + RewardText(lv, "프리미엄"));
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
