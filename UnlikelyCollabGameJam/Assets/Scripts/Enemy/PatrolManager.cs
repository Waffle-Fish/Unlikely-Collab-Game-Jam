using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class PatrolManager : MonoBehaviour
{
    protected List<Vector2> patrolPoints = new List<Vector2>();
    protected List<Vector2> path = new List<Vector2>();

    public LayerMask detectionLayer;
    public float neighborThreshold = 3.5f;

    [SerializeField]
    protected GameObject ppObject;

    [Header("Initialize Patrol Points")]
    [SerializeField] int xStart = -10;
    [SerializeField] int yStart = 10;
    [SerializeField] int width = 60;
    [SerializeField] int height = 12;

    private LineRenderer lr;

    protected virtual void Awake()
    {
        InitializePatrolPoints(xStart, yStart, width, height);
        detectionLayer = LayerMask.GetMask("Default");
        lr = GetComponent<LineRenderer>();
    }

    protected virtual void InitializePatrolPoints(int xStart, int yStart, int width, int height)
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
                        patrolPoints.Add(point);
                        // Just for visualization purposes
                        // Instantiate(ppObject, point, Quaternion.identity);
                    }
                }
            }
        }
    }

    protected Vector2 FindRandomClosePatrolPoint(Vector2 enemyLocation, float threshold = 5f)
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

    protected virtual void SetNextPatrolPath(Vector2 enemyLocation)
    {
        path = new List<Vector2>();

        // 1. Choose a starting point near the enemy.
        Vector2 start = FindRandomClosePatrolPoint(enemyLocation, 5f);

        // 2. Choose a goal from points that are a bit farther away.
        List<Vector2> candidateGoals = patrolPoints.Where(p => Vector2.Distance(p, start) > 3f).ToList();
        Vector2 goal;
        if (candidateGoals.Count > 0)
        {
            goal = candidateGoals[Random.Range(0, candidateGoals.Count)];
        }
        else
        {
            List<Vector2> otherPoints = patrolPoints.Where(p => p != start).ToList();
            goal = otherPoints.Count > 0 ? otherPoints[Random.Range(0, otherPoints.Count)] : start;
        }

        // Build the path using a greedy, step-by-step method.
        Vector2 current = start;
        path.Add(current);

        // Safety counter to prevent potential infinite loops.
        int safetyCounter = 0;
        const int maxIterations = 1000;
        // Continue until we're close enough to the goal.
        while (Vector2.Distance(current, goal) > 1f && safetyCounter < maxIterations)
        {
            // Get all neighbors of the current point.
            List<Vector2> neighbors = GetNeighbors(current);

            // Define the direction from current toward the goal.
            Vector2 directionToGoal = (goal - current).normalized;

            // Filter neighbors to those that are roughly in the direction of the goal.
            List<Vector2> directionalNeighbors = neighbors
                .Where(n => Vector2.Dot((n - current).normalized, directionToGoal) > 0.5f)
                .ToList();

            // If no neighbor meets the directional requirement, fall back to all neighbors.
            if (directionalNeighbors.Count == 0)
            {
                directionalNeighbors = neighbors;
            }

            // If still no neighbor is available, break out (dead end).
            if (directionalNeighbors.Count == 0)
            {
                break;
            }

            // Choose the neighbor that is closest to current.
            Vector2 next = directionalNeighbors.OrderBy(n => Vector2.Distance(current, n)).First();

            // Avoid cycles: if we've already visited this point, break to avoid looping.
            if (path.Contains(next))
            {
                break;
            }

            // Add the chosen neighbor to the path.
            current = next;
            path.Add(current);
            safetyCounter++;
        }

        // Ensure the goal is the final point if we're not already close.
        if (Vector2.Distance(current, goal) > 1f)
        {
            path.Add(goal);
        }
    }

    // Returns neighbors within a set threshold.
    protected List<Vector2> GetNeighbors(Vector2 point)
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

    public void DrawDebugPath(Vector2 enemyLocation)
    {
        // Convert Vector2 list to Vector3 list and add enemyLocation as start.
        List<Vector3> pathV3 = path.Select(v2 => (Vector3)v2).ToList();
        pathV3.Insert(0, (Vector3)enemyLocation);

        lr.positionCount = pathV3.Count;
        lr.SetPositions(pathV3.ToArray());
    }

    // Returns the next patrol point in the computed path.
    public Vector2 GetNextPatrolPointInPath(Vector2 enemyLocation)
    {
        if (path != null && path.Count > 0)
        {
            Vector2 nextPoint = path[0];
            path.RemoveAt(0);
            return nextPoint;
        }
        else
        {
            SetNextPatrolPath(enemyLocation);
            return GetNextPatrolPointInPath(enemyLocation);
        }
    }
}
