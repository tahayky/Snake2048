using System.Collections.Generic;
using UnityEngine;

namespace Snake2048
{
    public class UIManager : MonoBehaviour
    {
        public ScoreBoard scoreBoard;

        public void UpdateScoreBoard(ref IScorable[] characters,IScorable target)
        {
            scoreBoard.UpdateBoard(ref characters,target);
            
        }
    }
}
