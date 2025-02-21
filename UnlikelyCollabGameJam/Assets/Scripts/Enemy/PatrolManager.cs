using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;


public class PatrolManager : MonoBehaviour
{

    private List<Vector2> patrolPoints = new List<Vector2>();
    private List<Vector2> path = new List<Vector2>();

    public LayerMask detectionLayer;
    public float neighborThreshold = 3.5f;

    [SerializeField]
    private GameObject ppObject;


    public void Awake()
    {
        detectionLayer = LayerMask.GetMask("Default");
        InitializePatrolPoints(-10, 10, 20, 8);
    }

    private void InitializePatrolPoints(int xStart, int yStart, int width, int height)
    {
        for (int i = xStart; i < width + xStart; i += 2)
        {
            for (int j = yStart; j > yStart - height; j -= 2)
            {
                Vector2 point = new Vector2(i, j);

                RaycastHit2D hit = Physics2D.Raycast(point, Vector2.down, 30f, detectionLayer);
                
                if (hit.collider != null)
                {
                    point = hit.point;
                    if (!patrolPoints.Contains(point))
                    {
                        Instantiate(ppObject, point, Quaternion.identity);
                        patrolPoints.Add(point);
                    }
                }
            }
        }

        Debug.Log("PatrolPoints Count: "+patrolPoints.Count);
    }

    public void SetNextPatrolPath(Vector2 enemyLocation)
    {
        path = new List<Vector2>();

        // 1. Use the closest patrol point as the start.
        Vector2 start = FindClosestPatrolPoint(enemyLocation);

        // 2. Choose a goal among points that are sufficiently above the start.
        List<Vector2> higherPoints = patrolPoints.FindAll(p => p.y > start.y + 1f);
        Vector2 goal;
        if (higherPoints.Count > 0)
        {
            goal = higherPoints[Random.Range(0, higherPoints.Count)];
        }
        else
        {
            // Fallback: choose the highest patrol point available.
            goal = patrolPoints.OrderBy(p => p.y).Last();
        }

        // Initialize dictionaries for A*.
        Dictionary<Vector2, float> gScore = new Dictionary<Vector2, float>();
        Dictionary<Vector2, float> fScore = new Dictionary<Vector2, float>();
        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();

        List<Vector2> openSet = new List<Vector2>();

        foreach (var point in patrolPoints)
        {
            gScore[point] = Mathf.Infinity;
            fScore[point] = Mathf.Infinity;
        }
        gScore[start] = 0;
        fScore[start] = HeuristicCostEstimate(start, goal);
        openSet.Add(start);

        // Factor to penalize downward moves.
        float verticalPenaltyFactor = 10f;

        while (openSet.Count > 0)
        {
            // Get the node with the lowest fScore.
            Vector2 current = openSet[0];
            foreach (var point in openSet)
            {
                if (fScore[point] < fScore[current])
                {
                    current = point;
                }
            }

            // If the goal is reached, reconstruct the path.
            if (current == goal)
            {
                path = ReconstructPath(cameFrom, current);
                return;
            }

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                // Calculate the movement cost.
                float transitionCost = Vector2.Distance(current, neighbor);
                // Add penalty if the neighbor is lower than the current node.
                if (neighbor.y < current.y)
            {
                transitionCost += (current.y - neighbor.y) * verticalPenaltyFactor;
            }
            float tentativeGScore = gScore[current] + transitionCost;

            if (tentativeGScore < gScore[neighbor])
            {
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = tentativeGScore + HeuristicCostEstimate(neighbor, goal);
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
            }
        }
    }

    Debug.LogWarning("No patrol path found between " + start + " and " + goal);
}

// Helper function to find the closest patrol point.
private Vector2 FindClosestPatrolPoint(Vector2 enemyLocation)
{
    Vector2 closest = patrolPoints[0];
    float minDist = Vector2.Distance(enemyLocation, closest);
    foreach (Vector2 point in patrolPoints)
    {
        float dist = Vector2.Distance(enemyLocation, point);
        if (dist < minDist)
        {
            closest = point;
            minDist = dist;
        }
    }
    return closest;
}


    // Heuristic function: Euclidean distance between two patrol points.
    private float HeuristicCostEstimate(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }

    // Returns all patrol points that are within the neighborThreshold distance.
    private List<Vector2> GetNeighbors(Vector2 point)
    {
        List<Vector2> neighbors = new List<Vector2>();
        foreach (var other in patrolPoints)
        {
            if (other != point)
            {
                if (Vector2.Distance(point, other) <= neighborThreshold)
                {
                    neighbors.Add(other);
                }
            }
        }
        return neighbors;
    }

    // Reconstructs the path from start to goal using the cameFrom dictionary.
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

    // Returns the next patrol point in the computed path.
    public Vector2 GetNextPatrolPointInPath(Vector2 enemyLocation)
    {
        Debug.Log("PATH LENGTH:"+path.Count);
        if (path != null && path.Count > 0)
        {
            Vector2 nextPoint = path[0];
            path.RemoveAt(0);
            Debug.Log("Going to: "+nextPoint);
            return nextPoint;
        }
        else
        {
            SetNextPatrolPath(enemyLocation);
            return GetNextPatrolPointInPath(enemyLocation);
        }
        // return new Vector2(0f, 0f);
    }
}
