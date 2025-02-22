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

    private LineRenderer lr;

    public void Awake()
    {
        detectionLayer = LayerMask.GetMask("Default");
        lr = GetComponent<LineRenderer>();
        InitializePatrolPoints(-10, 10, 60, 12);
    }

    private void InitializePatrolPoints(int xStart, int yStart, int width, int height)
    {
        for (int i = xStart; i < width + xStart; i += 1)
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
        Debug.Log("PatrolPoints Count: " + patrolPoints.Count);
    }

    // NEW: Choose a start that's "close" to the enemy, but not necessarily the absolute closest.
    private Vector2 FindRandomClosePatrolPoint(Vector2 enemyLocation, float threshold = 5f)
    {
        List<Vector2> closePoints = patrolPoints.Where(p => Vector2.Distance(enemyLocation, p) <= threshold).ToList();
        if (closePoints.Count > 0)
        {
            return closePoints[Random.Range(0, closePoints.Count)];
        }
        else
        {
            return FindClosestPatrolPoint(enemyLocation);
        }
    }

    // The old absolute closest method (used as a fallback).
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

    // Modified SetNextPatrolPath that does not force an ascending route.
    public void SetNextPatrolPath(Vector2 enemyLocation)
    {
        path = new List<Vector2>();

        // 1. Pick a starting point from those that are "close" to the enemy.
        Vector2 start = FindRandomClosePatrolPoint(enemyLocation, 5f);

        // 2. Choose a goal randomly among patrol points that are a little farther from start.
        List<Vector2> candidateGoals = patrolPoints.Where(p => Vector2.Distance(p, start) > 3f).ToList();
        Vector2 goal;
        if (candidateGoals.Count > 0)
        {
            goal = candidateGoals[Random.Range(0, candidateGoals.Count)];
        }
        else
        {
            // Fallback: choose any random patrol point different from start.
            List<Vector2> otherPoints = patrolPoints.Where(p => p != start).ToList();
            goal = otherPoints.Count > 0 ? otherPoints[Random.Range(0, otherPoints.Count)] : start;
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

        // Lower vertical penalty to allow downward movement.
        float verticalPenaltyFactor = 0f; // Set to 0 to remove bias toward ascending

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
                // Optionally add vertical penalty if desired; here we remove it.
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

    // Heuristic function: Euclidean distance.
    private float HeuristicCostEstimate(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }

    // Returns neighbors within a set threshold.
    private List<Vector2> GetNeighbors(Vector2 point)
    {
        List<Vector2> neighbors = new List<Vector2>();
        foreach (var other in patrolPoints)
        {
            if (other != point && Vector2.Distance(point, other) <= neighborThreshold)
            {
                neighbors.Add(other);
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

    public void DrawDebugPath(Vector2 enemyLocation)
    {
        // if (path == null || path.Count < 2) return;

        // Convert Vector2 list to Vector3 list and add enemyLocation as start.
        List<Vector3> pathV3 = path.Select(v2 => (Vector3)v2).ToList();
        pathV3.Insert(0, (Vector3)enemyLocation);

        lr.positionCount = pathV3.Count;
        lr.SetPositions(pathV3.ToArray());
    }

    // Returns the next patrol point in the computed path.
    public Vector2 GetNextPatrolPointInPath(Vector2 enemyLocation)
    {
        Debug.Log("PATH LENGTH:" + path.Count);
        if (path != null && path.Count > 0)
        {
            Vector2 nextPoint = path[0];
            path.RemoveAt(0);
            // DrawDebugPath(enemyLocation);
            Debug.Log("Going to: " + nextPoint);
            return nextPoint;
        }
        else
        {
            SetNextPatrolPath(enemyLocation);
            return GetNextPatrolPointInPath(enemyLocation);
        }
    }
}
