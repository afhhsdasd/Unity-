
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class KDTree
{
    public Vector2 Pos;
    public int Axis;
    public KDTree Left;
    public KDTree Right;

    public KDTree(Vector2 pos, int axis)
    {
        Pos = pos;
        Axis = axis;
        Left = null;
        Right = null;
    }

    public static KDTree BuildKDTree(List<Vector2> positions, int depth)
    {
        if(positions.Count == 0) return null;
        int axis = depth % 2;

        positions.Sort ((a, b) => axis == 0? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

        int mediumCount = positions.Count / 2;
        Vector2 mediumPos = positions[mediumCount];

        KDTree node = new (mediumPos,axis);
        node.Left = BuildKDTree (positions.GetRange (0, mediumCount), depth + 1);
        node.Right = BuildKDTree (positions.GetRange (mediumCount + 1, positions.Count- mediumCount - 1), depth + 1);

        return node;
    }

    public Vector2 FindNearest(KDTree node , Vector2 target, int depth)
    {
        if(node == null) return Vector2.zero;

        int axis = depth % 2;
        KDTree nextBranch = (axis == 0 ? target.x < node.Pos.x : target.y < node.Pos.y) ? node.Left : node.Right;
        KDTree oppositeBranch = (nextBranch == node.Left) ? node.Right : node.Left;

        Vector2 best = FindNearest (nextBranch, target, depth + 1);

        if(best == Vector2.zero || DistanceSquared (node.Pos, target) < DistanceSquared (best, target))
        {
            best = node.Pos;
        }

        // 如果另一子树可能有更近的点
        if(oppositeBranch != null)
        {
            float distToSplit = axis == 0 ? Mathf.Abs (target.x - node.Pos.x) : Mathf.Abs (target.y - node.Pos.y);
            if(distToSplit * distToSplit < DistanceSquared (best, target))
            {
                Vector2 possibleBest = FindNearest (oppositeBranch, target, depth + 1);
                if(possibleBest != Vector2.zero && DistanceSquared (possibleBest, target) < DistanceSquared (best, target))
                {
                    best = possibleBest; 
                }
            }
        }

        return best;
    }

    private float DistanceSquared(Vector2 a, Vector2 b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return dx * dx + dy * dy;
    }


}