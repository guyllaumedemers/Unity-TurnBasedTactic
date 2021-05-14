using Globals;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class AStar : MonoBehaviour
{
    public Unit selected;
    public bool moveCompleted;
    public List<Vector3> GraphNodes { get; private set; }

    public float movementSpeed;
    public GameObject nodePrefab;
    public LayerMask layerMask;

    List<GameObject> possibleMoves = new List<GameObject>();
    public static int movementCost;

    private void Awake()
    {
        GraphNodes = new List<Vector3>();
    }

    /// <summary>
    /// This method draws the "real-time" visual overlay when moving the mouse over the available nodes.
    /// </summary>
    public void HandlePathNodeSelection(System.Action actionCompleted = null)
    {
        AStarNode button = null;
        if (selected != null)
        {
            if (selected is PlayerUnit)
            {
                button = GridManager.GetObjectByRay<AStarNode>(InputManager.Instance.currentRayMousePos);
            }
            else
            {
                Vector3 v = GridManager.GetTileToCellFromWorld((selected as AIEnemy).target);
                Vector3 v1 = new Vector3(v.x, v.y, Camera.main.transform.position.z);
                button = GridManager.GetObjectByRay<AStarNode>(v1);
            }
        }
        if (button != null)
        {
            StartCoroutine(HandlePathSelection(button.node, actionCompleted));
        }
    }


    public void Select(Unit unit)
    {
        selected = unit;
    }

    public void GenerateMoves()
    {
        if (selected != null)
        {
            DestroyPossibleMoves();
            GeneratePossibleMoves();
        }
    }

    public void Deselect()
    {
        if (selected != null)
            selected = null;

        DestroyPossibleMoves();
        GridManager.ClearTiles();
    }
    /// <summary>
    /// This method calculate the path that will be taken by the unit and also shows the path nodes that will be used.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private IEnumerator HandlePathSelection(GraphNode node, System.Action actionCompleted = null)
    {
        moveCompleted = false;

        ABPath path = ABPath.Construct(selected.transform.position, (Vector3)node.position);

        path.traversalProvider = selected.traversalProvider;

        AstarPath.StartPath(path);

        while (path.vectorPath.Count == 0)
        {
            StartCoroutine(path.WaitForPath());
        }

        if (path.error)
        {
            Debug.LogError("Path failed:\n" + path.errorLog);
            GeneratePossibleMoves();
            yield break;
        }

        for (int i = 0; i < path.vectorPath.Count; i++)
        {
            AddCurrentPathToOverlay(path.vectorPath[i]);
        }

        movementCost = GridManager.PathNodesOverlayTiles.Count - 1;
        GridManager.DrawCurrentPathNodes();

        /// check for the input behaviours when the AI is playing
        if ((InputManager.Instance.canUseInputs && Input.GetMouseButtonDown(0)) ^ (selected is AIEnemy))
        {
            /// Disable Inputs for player
            DisableInputs(selected, false);
            /// Select target node for AStar
            selected.targetNode = path.path[path.path.Count - 1];
            /// Clear Overlay Tiles
            GridManager.ClearTiles();
            /// Destroy Previous Tiles Overlay
            DestroyPossibleMoves();
            /// Move along path AStar
            yield return StartCoroutine(MoveAlongPath(selected, path, movementSpeed));
            /// Enable Inputs
            DisableInputs(selected, true);
            /// Deselect AStar Unit
            Deselect();
            /// Confirm that the move is complete
            moveCompleted = true;
            actionCompleted?.Invoke();
        }
    }

    private void DisableInputs(Unit unit, bool input)
    {
        if (UnitManager.Instance.CheckUnitType<AIEnemy>(unit))
            return;
        InputManager.Instance.canUseInputs = input;
    }

    /// <summary>
    /// This method is called when the player clicks on an available pathnode and start moving the unit on the calculated path.
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="path"></param>
    /// <param name="speed"></param>
    /// <returns></returns>

    private IEnumerator MoveAlongPath(Unit unit, ABPath path, float speed)
    {
        if (path.error || path.vectorPath.Count == 0)
            throw new System.ArgumentException("Cannot follow an empty path");

        unit.RemoveAP(movementCost);

        float distanceAlongSegment = 0;
        for (int i = 0; i < path.vectorPath.Count - 1; i++)
        {
            Vector3 p0 = path.vectorPath[Mathf.Max(i - 1, 0)];
            Vector3 p1 = path.vectorPath[i];
            Vector3 p2 = path.vectorPath[i + 1];
            Vector3 p3 = path.vectorPath[Mathf.Min(i + 2, path.vectorPath.Count - 1)];

            float segmentLength = Vector3.Distance(p1, p2);
            ///rotate before move
            unit.positionGrid = GridManager.GetWorldToCellFromTile(unit.transform.position);
            if (unit.positionGrid.Equals(GridManager.GetWorldToCellFromTile(p1)) && i < path.vectorPath.Count - 2)
            {
                Vector3Int pos = GridManager.GetWorldToCellFromTile(p2);
                unit.SetSpriteOrientation(pos);
            }
            while (distanceAlongSegment < segmentLength)
            {
                Vector3 interpolatedPoint = AstarSplines.CatmullRom(p0, p1, p2, p3, distanceAlongSegment / segmentLength);
                unit.transform.position = interpolatedPoint;
                yield return null;
                distanceAlongSegment += Time.deltaTime * speed;
            }

            distanceAlongSegment -= segmentLength;
        }

        unit.transform.position = path.vectorPath[path.vectorPath.Count - 1];
        unit.positionGrid = GridManager.GetWorldToCellFromTile(unit.transform.position);

        unit.blocker.BlockAtCurrentPosition();
        unit.animationManager.Animate(false, AnimationManager.moveTrigger);
    }

    private void DestroyPossibleMoves()
    {
        foreach (GameObject go in possibleMoves)
            Destroy(go);

        possibleMoves.Clear();
    }

    /// <summary>
    /// This method generates all the nodes available for the pathfinding (no visual overlays).
    /// </summary>
    private void GeneratePossibleMoves()
    {
        GraphNodes.Clear();

        ConstantPath path = ConstantPath.Construct(selected.transform.position, selected.unitData.stats.Ap * 750);
        path.traversalProvider = selected.traversalProvider;

        AstarPath.StartPath(path);

        path.BlockUntilCalculated();

        foreach (GraphNode node in path.allNodes)
        {
            if (node != path.startNode)
            {
                GameObject go = Instantiate(nodePrefab, (Vector3)node.position, Quaternion.identity);
                possibleMoves.Add(go);

                go.GetComponent<AStarNode>().node = node;
                node.position.z = 0;

                GraphNodes.Add((Vector3)node.position);
            }
        }
    }

    /// <summary>
    /// Builds a list of all available pathNodes to show a visual overlay.
    /// </summary>
    /// <param name="pathNode"></param>
    private void AddCurrentPathToOverlay(Vector3 pathNode)
    {
        Vector3Int nodePositionOnGrid = GridManager.GetWorldToCellFromTile(pathNode);
        GridManager.PathNodesOverlayTiles.Add(nodePositionOnGrid);
    }
    public bool CheckIfEnoughActionPoint(Unit unit, Vector3Int targetTilePos, Unit target = null)
    {
        selected = unit;
        GeneratePossibleMoves();
        DestroyPossibleMoves();
        if (target != null)
        {
            int index = Random.Range(0, GraphNodes.Count);
            (unit as AIEnemy).target = GridManager.GetWorldToCellFromTile(GraphNodes[index]);
        }
        foreach (Vector3 nodePosition in GraphNodes)
        {
            if (GridManager.GetWorldToCellFromTile(nodePosition).Equals(targetTilePos))
            {
                return true;
            }
        }

        return false;
    }
    public IEnumerator GetPathCost(Unit unit, Vector3Int targetPosition)
    {
        selected = unit;
        GeneratePossibleMoves();
        Vector3 start = GridManager.GetTileToCellFromWorld(unit.positionGrid);
        Vector3 end = GridManager.GetTileToCellFromWorld(targetPosition);
        ABPath path = ABPath.Construct(unit.transform.position, end);
        path.traversalProvider = unit.traversalProvider;
        AstarPath.StartPath(path);
        while (path.vectorPath.Count == 0)
        {
            StartCoroutine(path.WaitForPath());
        }
        //yield return StartCoroutine(path.WaitForPath());
        if (path.error)
        {
            Debug.LogError("Path failed:\n" + path.errorLog);
            GeneratePossibleMoves();
            yield break;
        }
        movementCost = path.vectorPath.Count - 1;
        //Debug.Log(unit.gameObject.name + " postion: " + unit.positionGrid + " target postion: " + targetPosition + " cost: " + (path.vectorPath.Count - 1));
        DestroyPossibleMoves();
    }
}
