using System;
using System.Collections.Generic;
using ColorBlockJam.Models;
using ColorBlockJam.Signals;
using UnityEngine;

namespace ColorBlockJam.Controllers
{
    public class GameController
    {
        private readonly SignalCenter _signalCenter;

        private readonly List<LevelInfo> _levelInfos;
        
        public GameController(SignalCenter signalCenter, List<LevelInfo> levelInfos)
        {
            _signalCenter = signalCenter;
            _levelInfos = levelInfos;
        }

        public void StartGame()
        {
            int levelIndex = GetCurrentLevel();
            
            if (levelIndex < 0)
            {
                throw new ArgumentException($"Invalid levelIndex. {levelIndex}");
            }

            if (levelIndex >= _levelInfos.Count)
            {
                levelIndex = 0;
                
                SetCurrentLevel(levelIndex);
            }
            
            _signalCenter.Fire(new StartGameRequestedSignal(_levelInfos[levelIndex]));
        }

        public void RestartGame()
        {
            _signalCenter.Fire(new RestartGameRequestedSignal());
        }

        public void NextLevel()
        {
            SetCurrentLevel(GetCurrentLevel() + 1);
            
            StartGame();
        }

        private int GetCurrentLevel()
        {
            return PlayerPrefs.GetInt("CurrentLevel", 0);
        }

        private void SetCurrentLevel(int level)
        {
            PlayerPrefs.SetInt("CurrentLevel", level);
        }
    }
}