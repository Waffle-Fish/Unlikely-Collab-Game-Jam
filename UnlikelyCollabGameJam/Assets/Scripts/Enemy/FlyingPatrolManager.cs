using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class FlyingPatrolManager : PatrolManager
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void InitializePatrolPoints(int xStart, int yStart, int width, int height)
    {
        for (int i = xStart; i < width + xStart; i += 10)
        {
            for (int j = yStart; j > yStart - height; j -= 6)
            {
                Vector2 point = new Vector2(i+Random.Range(-1f, 1f), j+Random.Range(-1f, 1f));
                patrolPoints.Add(point);
                // Just for visualization purposes
                Instantiate(ppObject, point, Quaternion.identity);
            }
        }
    }


    // protected override void SetNextPatrolPath(Vector2 enemyLocation)
    // {
    //     path = new List<Vector2>();

    //     // 1. Pick a starting point from those that are "close" to the enemy.
    //     Vector2 start = FindRandomClosePatrolPoint(enemyLocation, 5f);

    //     // 2. Choose a goal randomly among patrol points that are a little farther from start.
    //     List<Vector2> candidateGoals = patrolPoints.Where(p => Vector2.Distance(p, start) > 3f).ToList();
    //     Vector2 goal;
    //     if (candidateGoals.Count > 0)
    //     {
    //         goal = candidateGoals[Random.Range(0, candidateGoals.Count)];
    //     }
    //     else
    //     {
    //         // Fallback: choose any random patrol point different from start.
    //         List<Vector2> otherPoints = patrolPoints.Where(p => p != start).ToList();
    //         goal = otherPoints.Count > 0 ? otherPoints[Random.Range(0, otherPoints.Count)] : start;
    //     }

    //     // Initialize dictionaries for A*.
    //     Dictionary<Vector2, float> gScore = new Dictionary<Vector2, float>();
    //     Dictionary<Vector2, float> fScore = new Dictionary<Vector2, float>();
    //     Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();
    //     List<Vector2> openSet = new List<Vector2>();

    //     foreach (var point in patrolPoints)
    //     {
    //         gScore[point] = Mathf.Infinity;
    //         fScore[point] = Mathf.Infinity;
    //     }
    //     gScore[start] = 0;
    //     fScore[start] = HeuristicCostEstimate(start, goal);
    //     openSet.Add(start);

    //     // Lower vertical penalty to allow downward movement.
    //     float verticalPenaltyFactor = 0f; // Set to 0 to remove bias toward ascending

    //     while (openSet.Count > 0)
    //     {
    //         // Get the node with the lowest fScore.
    //         Vector2 current = openSet[0];
    //         foreach (var point in openSet)
    //         {
    //             if (fScore[point] < fScore[current])
    //             {
    //                 current = point;
    //             }
    //         }

    //         // If the goal is reached, reconstruct the path.
    //         if (current == goal)
    //         {
    //             path = ReconstructPath(cameFrom, current);
    //             return;
    //         }

    //         openSet.Remove(current);

    //         foreach (var neighbor in GetNeighbors(current))
    //         {
    //             // Calculate the movement cost.
    //             float transitionCost = Vector2.Distance(current, neighbor);
    //             // Optionally add vertical penalty if desired; here we remove it.
    //             if (neighbor.y < current.y)
    //             {
    //                 transitionCost += (current.y - neighbor.y) * verticalPenaltyFactor;
    //             }
    //             float tentativeGScore = gScore[current] + transitionCost;

    //             if (tentativeGScore < gScore[neighbor])
    //             {
    //                 cameFrom[neighbor] = current;
    //                 gScore[neighbor] = tentativeGScore;
    //                 fScore[neighbor] = tentativeGScore + HeuristicCostEstimate(neighbor, goal);
    //                 if (!openSet.Contains(neighbor))
    //                 {
    //                     openSet.Add(neighbor);
    //                 }
    //             }
    //         }
    //     }

    //     Debug.LogWarning("No patrol path found between " + start + " and " + goal);
    // }

     // Heuristic function: Euclidean distance.
    private float HeuristicCostEstimate(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }

    private List<Vector2> ReconstructPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 current)
    {
        List<Vector2> totalPath = new List<Vector2>();
        totalPath.Add(current);
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }
    

}
