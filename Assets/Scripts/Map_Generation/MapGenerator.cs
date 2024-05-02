using System; 
using UnityEngine; 

public class MapGenerator : MonoBehaviour // Declaring a class named MapGenerator, inheriting from MonoBehaviour
{
    [Range(0, 100)]
    public int RandFill;    // Public variable for controlling the random fill percentage
    public int width;       // Public variable for map width
    public int height;      // Public variable for map height
    public string seed;     // Public variable for map seed
    public bool UseRandomSeed = true; // Public variable for using random seed
    int[,] map;             // 2D array to store the map data

    void Start() // Start method called when the script is started
    {
        GenerateMap();
    }

    private void FixedUpdate()
    {
        //GenerateMap(); // Call GenerateMap method (Uncomment if you wnat the map to be generated every frame) [Resource Intensive]
    }


    void GenerateMap() // Method to generate the map
    {
        map = new int[width, height]; // Initialize the map array with given width and height
        RandomFillMap(); // Call the method to randomly fill the map
        for (int i = 0; i < 5; i++) // Loop for smoothing the map
        {
            SmoothMap(); // Call the method to smooth the map
        }
        int BorderSize = 1;

        int[,] BorderedMap = new int[width + BorderSize * 2, height + BorderSize * 2];

        for (int x = 0; x < BorderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < BorderedMap.GetLength(1); y++)
            {
                if (x >= BorderSize && x < width + BorderSize && y >= BorderSize && y < height + BorderSize)
                {
                    BorderedMap[x, y] = map[x - BorderSize, y - BorderSize];
                }
                else
                {
                    BorderedMap[x, y] = 1;
                }
            }

            MeshGenerator MeshGen = GetComponent<MeshGenerator>(); // Get the MeshGenerator component attached to this GameObject
            MeshGen.GenerateMesh(BorderedMap, 1); // Generate the mesh based on the map data
        }

        void RandomFillMap() // Method to randomly fill the map
        {
            if (UseRandomSeed) // Check if using random seed
            {
                seed = Time.time.ToString(); // Set the seed based on current time
            }

            System.Random PseudoRand = new(seed.GetHashCode()); // Create a pseudo-random generator with the seed

            for (int x = 0; x < width; x++) // Loop through map width
            {
                for (int y = 0; y < height; y++) // Loop through map height
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)              // Check if on the edge of the map
                    {
                        map[x, y] = 1; // Set the edge of the map to be solid
                    }
                    else
                    {
                        map[x, y] = (PseudoRand.Next(0, 100) < RandFill) ? 1 : 0;               // Randomly fill the inner part of the map based on RandFill percentage
                    }
                }
            }
        }

        void SmoothMap() // Method to smooth the map
        {
            for (int x = 0; x < width; x++) // Loop through map width
            {
                for (int y = 0; y < height; y++) // Loop through map height
                {
                    int neighbouringTiles = GetNeighboringWallCount(x, y);                      // Get the count of neighboring wall tiles
                    if (neighbouringTiles > 4)                                                  // If there are more than 4 wall neighbors
                        map[x, y] = 1;                                                          // Set the current tile to be a wall
                    else if (neighbouringTiles < 4)                                             // If there are less than 4 wall neighbors
                        map[x, y] = 0;                                                          // Set the current tile to be empty
                }
            }
        }

        int GetNeighboringWallCount(int gridX, int gridY) // Method to count neighboring wall tiles
        {
            int WallCount = 0; // Initialize the wall count
            for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) // Loop through neighboring tiles in x direction
            {
                for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) // Loop through neighboring tiles in y direction
                {
                    if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height) // Check if the neighboring tile is within the map boundaries
                    {
                        if (neighbourX != gridX || neighbourY != gridY) // Check if the neighboring tile is not the current tile
                        {
                            WallCount += map[neighbourX, neighbourY];   // Increment wall count if the neighboring tile is a wall
                        }
                    }
                    else // If the neighboring tile is outside the map boundaries
                    {
                        WallCount++; // Increment wall count
                    }
                }
            }
            return WallCount; // Return the total wall count
        }

    }
}