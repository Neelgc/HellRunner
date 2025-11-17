using System.Threading;
using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Procedural Generation Method/Underworld Map")]
public class UnderworldMap : ProceduralGenerationMethod
{
    [Header("Noise Parameters")]
    [SerializeField] private FastNoiseLite.NoiseType _noiseType = FastNoiseLite.NoiseType.Perlin;
    [SerializeField, Range(0.01f, 0.2f)] private float _frequency = 0.05f;
    [SerializeField, Range(0.5f, 1.5f)] private float _amplitude = 1f;

    [Header("Fractal Parameters")]
    [SerializeField] private FastNoiseLite.FractalType _fractalType = FastNoiseLite.FractalType.FBm;
    [SerializeField, Range(1, 6)] private int _octaves = 3;
    [SerializeField, Range(1f, 4f)] private float _lacunarity = 2f;
    [SerializeField, Range(0f, 1f)] private float _persistence = 0.5f;

    [Header("Heights - Plus la valeur est haute, plus c'est rare")]
    [SerializeField, Range(-1f, 1f)] private float _redRockHardHeight = -0.3f;
    [SerializeField, Range(-1f, 1f)] private float _redRockHeight = 0.2f; 
    [SerializeField, Range(-1f, 1f)] private float _redSandHeight = 0.5f;
    [SerializeField, Range(-1f, 1f)] private float _cavityHeight = 0.7f;

    [Header("Border Settings")]
    [SerializeField, Range(1, 5)] private int _borderThickness = 2;

    private int _chunkOffsetX = 0;

    public void SetChunkOffset(int offsetX)
    {
        _chunkOffsetX = offsetX;
    }

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        FastNoiseLite noise = new FastNoiseLite(RandomService.Seed);
        noise.SetNoiseType(_noiseType);
        noise.SetFrequency(_frequency);
        noise.SetFractalType(_fractalType);
        noise.SetFractalOctaves(_octaves);
        noise.SetFractalLacunarity(_lacunarity);
        noise.SetFractalGain(_persistence);

        float[,] noiseMap = new float[Grid.Width, Grid.Lenght];

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Lenght; y++)
            {
                int globalX = x + _chunkOffsetX;
                noiseMap[x, y] = noise.GetNoise(globalX, y) * _amplitude;
            }
        }

        for (int x = 0; x < Grid.Width; x++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (int y = 0; y < Grid.Lenght; y++)
            {
                if (!Grid.TryGetCellByCoordinates(x, y, out var cell))
                    continue;

                bool isBorder = x < _borderThickness ||
                               x >= Grid.Width - _borderThickness ||
                               y < _borderThickness ||
                               y >= Grid.Lenght - _borderThickness;

                if (isBorder)
                {
                    AddTileToCell(cell, RED_ROCK_TILE_NAME, true);
                }
                else
                {
                    float noiseValue = noiseMap[x, y];


                    if (noiseValue < _redRockHardHeight)
                    {
                        AddTileToCell(cell, RED_ROCK_HARD_TILE_NAME, true);
                    }
                    else if (noiseValue < _redRockHeight)
                    {
                        AddTileToCell(cell, RED_ROCK_TILE_NAME, true);
                    }
                    else if (noiseValue < _redSandHeight)
                    {
                        AddTileToCell(cell, RED_SAND_TILE_NAME, true);
                    }
                    
                    else if (noiseValue < _cavityHeight)
                    {
                        continue;

                    }
                }
            }

            if (x % 5 == 0)
            {
                await UniTask.Delay(GridGenerator.StepDelay / 10, cancellationToken: cancellationToken);
            }
        }
    }
}