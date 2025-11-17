using Components.ProceduralGeneration;
using System.Collections.Generic;
using UnityEngine;
using VTools.RandomService;
using Cysharp.Threading.Tasks;

public class ChunkManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _chunkPrefab;
    [SerializeField] private OverworldMap _generationMethodTemplate;
    [SerializeField] private UnderworldMap _underworldMethodTemplate;
    [SerializeField] private Transform _camera;

    [Header("Map Generation Options")]
    [SerializeField] private bool _generateOverworld = true;
    [SerializeField] private bool _generateUnderworld = true;
    [SerializeField, Range(0, 10)] private int _gapBetweenMaps = 0;


    [Header("Chunk Settings")]
    [SerializeField] private int _chunkWidth = 64;
    [SerializeField] private int _chunkHeight = 64;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private bool _useRandomSeed = true;
    [SerializeField] private int _seed = 0;

    [Header("Generation Trigger")]
    [SerializeField] private float _generateNextChunkDistance = 40f;
    [SerializeField] private float _minDistanceBeforeTransition = 10f; 

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true;

    private Dictionary<int, ChunkData> _activeChunks = new();
    private int _currentChunkIndex = 0;
    private int _nextChunkIndex = 1;
    private HashSet<int> _chunksBeingGenerated = new();

    private class ChunkData
    {
        public GameObject OverworldObject;
        public GameObject UnderworldObject;
        public bool IsOverworldGenerated;
        public bool IsUnderworldGenerated;
        public ProceduralGridGenerator OverworldGenerator;
        public ProceduralGridGenerator UnderworldGenerator;
    }

    private void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000);

        if (_useRandomSeed)
        {
            _seed = Random.Range(int.MinValue, int.MaxValue);
            if (_showDebugLogs)
                Debug.Log($"Seed aléatoire générée: {_seed}");
        }
        else if (_showDebugLogs)
        {
            Debug.Log($" Seed fixe utilisée: {_seed}");
        }

        GenerateChunkAsync(0).Forget();
        GenerateChunkAsync(1).Forget();
    }

    private void Update()
    {
        if (_camera == null) return;

        float cameraX = _camera.position.x;

        CheckAndGenerateNextChunk(cameraX);
        CheckChunkTransition(cameraX);
    }

    private void CheckAndGenerateNextChunk(float cameraX)
    {
        float nextChunkStartX = _nextChunkIndex * _chunkWidth * _cellSize ;
        float distanceToNextChunk = nextChunkStartX - cameraX;

        if (distanceToNextChunk <= _generateNextChunkDistance &&
            !_activeChunks.ContainsKey(_nextChunkIndex) &&
            !_chunksBeingGenerated.Contains(_nextChunkIndex))
        {
            GenerateChunkAsync(_nextChunkIndex).Forget();
        }
    }

    private void CheckChunkTransition(float cameraX)
    {
        float currentChunkEndX = (_currentChunkIndex + 1) * _chunkWidth * _cellSize;
        float distanceToChunkEnd = currentChunkEndX - cameraX;

        if (distanceToChunkEnd <= _minDistanceBeforeTransition)
        {
            if (_activeChunks.ContainsKey(_nextChunkIndex))
            {
                var nextChunk = _activeChunks[_nextChunkIndex];
                bool isReady = (!_generateOverworld || nextChunk.IsOverworldGenerated) &&
                              (!_generateUnderworld || nextChunk.IsUnderworldGenerated);

                if (isReady && cameraX >= currentChunkEndX)
                {
                    TransitionToNextChunk();
                }
            }
            else if (_showDebugLogs)
            {
                Debug.LogWarning($"Caméra proche de la fin du chunk {_currentChunkIndex}, mais le chunk {_nextChunkIndex} n'est pas encore prêt!");
            }
        }
    }

    private void TransitionToNextChunk()
    {
        if (_showDebugLogs)
            Debug.Log($"Transition du chunk {_currentChunkIndex} vers {_nextChunkIndex}");

        DestroyPreviousChunk();

        _currentChunkIndex++;
        _nextChunkIndex++;
    }

    private async UniTaskVoid GenerateChunkAsync(int chunkIndex)
    {
        if (_activeChunks.ContainsKey(chunkIndex))
        {
            Debug.LogWarning($"Chunk {chunkIndex} existe déjà!");
            return;
        }

        if (_chunksBeingGenerated.Contains(chunkIndex))
        {
            Debug.LogWarning($"Chunk {chunkIndex} est déjà en cours de génération!");
            return;
        }

        _chunksBeingGenerated.Add(chunkIndex);

        try
        {
            if (_showDebugLogs)
                Debug.Log($"Début génération du chunk {chunkIndex}");

            var chunkData = new ChunkData
            {
                IsOverworldGenerated = false,
                IsUnderworldGenerated = false
            };

            _activeChunks[chunkIndex] = chunkData;

            // Générer overworld
            if (_generateOverworld)
            {
                GameObject overworldChunk = Instantiate(_chunkPrefab, transform);
                overworldChunk.name = $"Chunk_Overworld_{chunkIndex}";

                Vector3 overworldPosition = new Vector3(chunkIndex * _chunkWidth * _cellSize, 0, 0);
                overworldChunk.transform.position = overworldPosition;

                var overworldGenerator = overworldChunk.GetComponent<ProceduralGridGenerator>();
                if (overworldGenerator == null)
                {
                    overworldGenerator = overworldChunk.AddComponent<ProceduralGridGenerator>();
                }

                var overworldMethod = Instantiate(_generationMethodTemplate);
                overworldMethod.SetChunkOffset(chunkIndex * _chunkWidth);

                chunkData.OverworldObject = overworldChunk;
                chunkData.OverworldGenerator = overworldGenerator;

                await SetupAndGenerateGrid(overworldGenerator, overworldMethod);
                chunkData.IsOverworldGenerated = true;

                if (_showDebugLogs)
                    Debug.Log($"Overworld chunk {chunkIndex} généré");
            }

            if (_generateUnderworld)
            {
                GameObject underworldChunk = Instantiate(_chunkPrefab, transform);
                underworldChunk.name = $"Chunk_Underworld_{chunkIndex}";

                Vector3 underworldPosition = new Vector3(
                    chunkIndex * _chunkWidth * _cellSize,
                    -(_chunkHeight + _gapBetweenMaps) * _cellSize, 
                    0
                );
                underworldChunk.transform.position = underworldPosition;

                var underworldGenerator = underworldChunk.GetComponent<ProceduralGridGenerator>();
                if (underworldGenerator == null)
                {
                    underworldGenerator = underworldChunk.AddComponent<ProceduralGridGenerator>();
                }

                var underworldMethod = Instantiate(_underworldMethodTemplate);
                underworldMethod.SetChunkOffset(chunkIndex * _chunkWidth);

                chunkData.UnderworldObject = underworldChunk;
                chunkData.UnderworldGenerator = underworldGenerator;

                await SetupAndGenerateGrid(underworldGenerator, underworldMethod);
                chunkData.IsUnderworldGenerated = true;

                if (_showDebugLogs)
                    Debug.Log($"Underworld chunk {chunkIndex} généré");
            }

            if (_showDebugLogs)
                Debug.Log($"Chunk {chunkIndex} entièrement généré");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur génération chunk {chunkIndex}: {e.Message}");

            if (_activeChunks.ContainsKey(chunkIndex))
            {
                var chunk = _activeChunks[chunkIndex];
                if (chunk.OverworldObject != null) Destroy(chunk.OverworldObject);
                if (chunk.UnderworldObject != null) Destroy(chunk.UnderworldObject);
                _activeChunks.Remove(chunkIndex);
            }
        }
        finally
        {
            _chunksBeingGenerated.Remove(chunkIndex);
        }
    }

    private async UniTask SetupAndGenerateGrid(ProceduralGridGenerator generator, ProceduralGenerationMethod method)
    {
        Vector3 chunkStartPosition = generator.transform.position;
        generator.SetStartPosition(chunkStartPosition);

        generator.SetGridSize(_chunkWidth, _chunkHeight);
        generator.SetCellSize(_cellSize);
        generator.SetSeed(_seed);
        generator.SetStepDelay(0);
        generator.SetGenerationMethod(method);

        generator.GenerateGridManually();

        await UniTask.Yield();
        await UniTask.Delay(100);
    }

    private void DestroyPreviousChunk()
    {
        int previousChunkIndex = _currentChunkIndex - 1;

        if (_activeChunks.ContainsKey(previousChunkIndex))
        {
            if (_showDebugLogs)
                Debug.Log($" Destruction du chunk {previousChunkIndex}");

            var chunk = _activeChunks[previousChunkIndex];
            if (chunk.OverworldObject != null) Destroy(chunk.OverworldObject);
            if (chunk.UnderworldObject != null) Destroy(chunk.UnderworldObject);

            _activeChunks.Remove(previousChunkIndex);
        }
    }

    private void OnDrawGizmos()
    {
        if (_camera == null) return;

        float cameraX = _camera.position.x;

        // position de la cam
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            new Vector3(cameraX, -5, 0),
            new Vector3(cameraX, 5, 0)
        );

        // limites du chunk actuel
        Gizmos.color = Color.green;
        float currentChunkStartX = _currentChunkIndex * _chunkWidth * _cellSize;
        float currentChunkEndX = (_currentChunkIndex + 1) * _chunkWidth * _cellSize;

        Gizmos.DrawLine(
            new Vector3(currentChunkStartX, -10, 0),
            new Vector3(currentChunkStartX, 10, 0)
        );
        Gizmos.DrawLine(
            new Vector3(currentChunkEndX, -10, 0),
            new Vector3(currentChunkEndX, 10, 0)
        );

        // zone de transition
        Gizmos.color = Color.red;
        float transitionX = currentChunkEndX - _minDistanceBeforeTransition;
        Gizmos.DrawLine(
            new Vector3(transitionX, -8, 0),
            new Vector3(transitionX, 8, 0)
        );

        // zone de génération prochain chunk
        Gizmos.color = Color.yellow;
        float nextChunkStartX = _nextChunkIndex * _chunkWidth * _cellSize;
        float triggerX = nextChunkStartX - _generateNextChunkDistance;
        Gizmos.DrawLine(
            new Vector3(triggerX, -10, 0),
            new Vector3(triggerX, 10, 0)
        );

        // chunks actifs
        foreach (var kvp in _activeChunks)
        {
            int index = kvp.Key;
            ChunkData chunkData = kvp.Value;

            float chunkX = index * _chunkWidth * _cellSize;

            // Dessiner chunk overworld
            if (_generateOverworld && chunkData.OverworldObject != null)
            {
                Gizmos.color = chunkData.IsOverworldGenerated ? Color.green : Color.gray;

                Vector3 bottomLeft = new Vector3(chunkX, 0, 0);
                Vector3 topRight = new Vector3(chunkX + _chunkWidth * _cellSize, _chunkHeight * _cellSize, 0);

                Gizmos.DrawWireCube(
                    (bottomLeft + topRight) / 2,
                    new Vector3(_chunkWidth * _cellSize, _chunkHeight * _cellSize, 0.1f)
                );
            }

            // Dessiner chunk underworld
            if (_generateUnderworld && chunkData.UnderworldObject != null)
            {
                Gizmos.color = chunkData.IsUnderworldGenerated ? new Color(1f, 0.5f, 0f) : Color.gray; 

                Vector3 bottomLeft = new Vector3(chunkX, -_gapBetweenMaps * _cellSize, 0);
                Vector3 topRight = new Vector3(chunkX + _chunkWidth * _cellSize, -_gapBetweenMaps * _cellSize + _chunkHeight * _cellSize, 0);

                Gizmos.DrawWireCube(
                    (bottomLeft + topRight) / 2,
                    new Vector3(_chunkWidth * _cellSize, _chunkHeight * _cellSize, 0.1f)
                );
            }
        }
    }

    private void OnDestroy()
    {
        // Nettoyer tous les chunks
        foreach (var chunk in _activeChunks.Values)
        {
            if (chunk.OverworldObject != null)
                Destroy(chunk.OverworldObject);
            if (chunk.UnderworldObject != null)
                Destroy(chunk.UnderworldObject);
        }
        _activeChunks.Clear();
        _chunksBeingGenerated.Clear();
    }

}