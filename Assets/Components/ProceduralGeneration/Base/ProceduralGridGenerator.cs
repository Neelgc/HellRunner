using System;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using System.Reflection;

namespace Components.ProceduralGeneration
{
    public class ProceduralGridGenerator : BaseGridGenerator
    {
        [Header("Generation Parameters")]
        [SerializeField] private ProceduralGenerationMethod _generationMethod;
        [SerializeField] private bool _drawDebug;
        [SerializeField] private int _seed = 1234;
        [SerializeField, Range(1,2000), Tooltip("Delay between each steps in milliseconds")] private int _stepDelay = 200;

        [Header("Manual Control")]
        [SerializeField] private bool _generateOnStart = false;


        public void SetStartPosition(Vector3 startPos)
        {
            var type = typeof(BaseGridGenerator);
            var startPosField = type.GetField("_startPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (startPosField != null)
            {
                startPosField.SetValue(this, startPos);
            }
            else
            {
                Debug.LogError("Impossible de trouver le champ _startPosition dans BaseGridGenerator");
            }
        }

        protected override void Start()
        {
            if (_generateOnStart)
            {
                base.Start();
            }
        }

        public int StepDelay => _stepDelay;

        public override void GenerateGrid()
        {
            Vector3 chunkPosition = transform.position;

            transform.position = Vector3.zero;

            base.GenerateGrid();

            transform.position = chunkPosition;

            if (_drawDebug)
                Grid.DrawGridDebug();

            ApplyGeneration();
        }

        private async void ApplyGeneration()
        {
            Debug.Log($"Starting generation {_generationMethod.name} ...");
            var time = DateTime.Now;
            
            _generationMethod.Initialize(this, new RandomService(_seed));
            await _generationMethod.Generate();
            
            Debug.Log($"Generation {_generationMethod.name} completed in {(DateTime.Now - time).TotalSeconds : 0.00} seconds.");
        }
        public void SetGridSize(int width, int height)
        {
            _gridXValue = width;
            _gridYValue = height;
        }

        public void SetGridOffset(Vector3 offset)
        {
            var type = typeof(BaseGridGenerator);
            var offsetField = type.GetField("_gridOffset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (offsetField != null)
            {
                offsetField.SetValue(this, offset);
            }
            else
            {
                Debug.LogWarning("BaseGridGenerator doesn't have _gridOffset field. Using transform position instead.");
            }
        }

        public void SetCellSize(float cellSize)
        {
            _cellSize = cellSize;
        }

        public void SetGenerationMethod(ProceduralGenerationMethod method)
        {
            _generationMethod = method;
        }

        public void SetSeed(int seed)
        {
            _seed = seed;
        }

        public void SetStepDelay(int delay)
        {
            _stepDelay = delay;
        }

        //  Méthode pour générer sans Start() automatique
        public void GenerateGridManually()
        {
            GenerateGrid();
        }
    }
}