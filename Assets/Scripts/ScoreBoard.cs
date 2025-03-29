using System;
using System.Collections.Generic;
using UnityEngine;

namespace Snake2048
{
    public class ScoreBoard : MonoBehaviour
    {
        public int cellCount;
        public ScoreBoardCell cellPrefab;
        ScoreBoardCell[] cells;
        bool initialized = false;
        private void Start()
        {
            cells=new ScoreBoardCell[cellCount];
            for (int i = 0; i < cellCount; i++)
            {
                cells[i] =  Instantiate(cellPrefab.gameObject,transform).GetComponent<ScoreBoardCell>();
            }
            initialized = true;
        }

        public void UpdateBoard(ref IScorable[] scorables,IScorable target)
        {
            if(!initialized) return;
            SortScorablesByScore(ref scorables);
            int targetIndex = Array.IndexOf(scorables,target);
            if (targetIndex > cellCount-1)
            {
                cells[^1].SetScore(targetIndex,1, target);
                int i = 0;
                for (var index = cellCount-2; index >= 0; index--)
                {
                    cells[index].SetScore(targetIndex-i-1,0, scorables[targetIndex-i-1]);
                    i++;
                }
            }
            else
            {
                for (var index = 0; index < cellCount; index++)
                {
                    int mark = 0;
                    if(index==targetIndex) mark = 1;
                    cells[index].SetScore(index+1,mark, scorables[index]);
                }
            }
        }
        
        public void SortScorablesByScore(ref IScorable[] scorables)
        {
            // Listeyi customValue'ya göre küçükten büyüğe sırala
            Array.Sort(scorables,(x, y) => y.Score.CompareTo(x.Score));
        }

    }
}
