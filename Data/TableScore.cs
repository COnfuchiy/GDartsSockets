using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GDartsSockets.Data
{
    public class TableScore
    {
        private Dictionary<string, List<int>> Table { get; set; } = new Dictionary<string, List<int>>();

        public static TableScore Create(IEnumerable<string> users, int startScore)
        {
            var tableScore = new TableScore();
            foreach (var user in users)
            {
                var listScore = new List<int> {startScore};
                tableScore.Table.Add(user, listScore);
            }

            return tableScore;
        }
        public TableScore()
        {
            
        }


        public List<int> GetPlayerScore(string player)
        {
            return Table[player];
        }

        public void SetPlayerScoreInStep(string player, int score)
        {
            var listScore = GetPlayerScore(player);
            var lastScore = listScore.Last();
            listScore.Add(lastScore - score);
        }

        public bool CheckOverGoal(string player, int score)
        {
            var listScore = GetPlayerScore(player).ToList();
            var lastScore = listScore.Last();
            return lastScore - score>=0;
        }

        public bool WinScoreCheck(string playerName)
        {
            var playerScore = GetPlayerScore(playerName).ToList();
            return playerScore.Last() == 0;
        }
        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}