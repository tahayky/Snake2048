using UnityEngine;

namespace Snake2048
{
    public class ScoreBoardCell : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI nameText;
        public NumberAbbreviation numberAbbreviation;

        public void SetScore(int index,int mark,IScorable score)
        {
            nameText.text = index.ToString()+"."+score.Name;
            numberAbbreviation.SetNumber(Mathf.FloorToInt(score.Score).ToString());
        }
    }
}
