using System.Threading;
using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Procedural Generation Method/Overworld Map")]
public class OverworldMap : ProceduralGenerationMethod
{
    [Header("Terrain Shape")]
    [SerializeField, Range(0.01f, 0.1f)] private float _terrainFrequency = 0.03f;
    [SerializeField, Range(0f, 1f)] private float _terrainAmplitude = 0.5f;
    [SerializeField, Range(1, 6)] private int _terrainOctaves = 3;
    [SerializeField, Range(1f, 4f)] private float _terrainLacunarity = 2f;
    [SerializeField, Range(0f, 1f)] private float _terrainPersistence = 0.5f;

    [Header("Height Settings")]
    [SerializeField, Range(0f, 1f)] private float _baseTerrainHeight = 0.4f; 
    [SerializeField] private int _minTerrainHeight = 5; 

    [Header("Mountain Settings")]
    [SerializeField, Range(1, 20)] private int _rockDepth = 5;
    [SerializeField, Range(0f, 1f)] private float _mountainThreshold = 0.6f;
    [SerializeField] private bool _onlyGrassOnTop = true;

    [Header("Noise Type")]
    [SerializeField] private FastNoiseLite.NoiseType _noiseType = FastNoiseLite.NoiseType.Perlin;
    [SerializeField] private FastNoiseLite.FractalType _fractalType = FastNoiseLite.FractalType.FBm;

    private float[] _heightMap;
    private int _chunkOffsetX = 0;

    public void SetChunkOffset(int offsetX)
    {
        _chunkOffsetX = offsetX;
    }

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        FastNoiseLite noise = new FastNoiseLite(RandomService.Seed);
        noise.SetNoiseType(_noiseType);
        noise.SetFrequency(_terrainFrequency);
        noise.SetFractalType(_fractalType);
        noise.SetFractalOctaves(_terrainOctaves);
        noise.SetFractalLacunarity(_terrainLacunarity);
        noise.SetFractalGain(_terrainPersistence);

        _heightMap = new float[Grid.Width];

        for (int x = 0; x < Grid.Width; x++)
        {
            int globalX = x + _chunkOffsetX;
            float noiseValue = noise.GetNoise(globalX, 0) * _terrainAmplitude;

            float normalizedHeight = (noiseValue + 1f) / 2f;
            normalizedHeight = Mathf.Lerp(_baseTerrainHeight, 1f, normalizedHeight);

            _heightMap[x] = normalizedHeight * Grid.Lenght;
            _heightMap[x] = Mathf.Max(_heightMap[x], _minTerrainHeight);
        }

        await ApplyHeightMapToGrid(cancellationToken);
    }

    private async UniTask ApplyHeightMapToGrid(CancellationToken cancellationToken)
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int surfaceHeight = Mathf.RoundToInt(_heightMap[x]);
            float normalizedHeight = _heightMap[x] / Grid.Lenght;

            for (int y = 0; y < surfaceHeight; y++)
            {
                if (!Grid.TryGetCellByCoordinates(x, y, out var cell))
                    continue;

                string tileName;

                if (y == surfaceHeight - 1)
                {
                    if (_onlyGrassOnTop)
                    {
                        tileName = GRASS_TILE_NAME; 
                    }
                    else
                    {
                        tileName = normalizedHeight >= _mountainThreshold ? GRASS_ON_ROCK_TILE_NAME : GRASS_TILE_NAME;
                    }
                }
                else
                {
                    // Sous la surface
                    if (normalizedHeight >= _mountainThreshold && y >= surfaceHeight - _rockDepth)
                    {
                        tileName = ROCK_TILE_NAME; 
                    }
                    else
                    {
                        tileName = DIRT_TILE_NAME;
                    }
                }

                AddTileToCell(cell, tileName, true);
            }

            if (x % 5 == 0) 
            {
                await UniTask.Delay(GridGenerator.StepDelay / 10, cancellationToken: cancellationToken);
            }
        }
    }
}