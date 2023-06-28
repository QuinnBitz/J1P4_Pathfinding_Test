using PathfindingDemo.Game.Enviroment;
using PathfindingDemo.Util.Physics;
using System.Diagnostics;

namespace PathfindingDemo.Game.Pathfinding;

internal class AStar : IPathfinding
{
    private List<Position> path;
    private List<Node> open = new();
    private List<Node> closed = new();

    public Position Start { get; set; }
    public Position Target { get; set; }
    public World World { get; init; }

    public AStar(World _world, Position _start, Position _target)
    {
        Start = _start;
        Target = _target;
        World = _world;

        path = new List<Position>();
    }

    /// <returns>
    /// the positions of the path
    /// </returns>
    public IReadOnlyCollection<Position> GetPathPositions()
    {
        //if the path has already been reconstructed, return the path
        if (path.Count > 0)
            return path;

        Node? node = closed[^1];
        while (node != null)
        {
            path.Add(node.Pos); //add the position to the path
            node = node.ParentNode; //set the current node to the parent node
        }

        //reverse the path so the first node is first
        path.Reverse();

        return path;
    }

    public Task Run()
    {
        int g = 0;      //the cost from the start node to the current node
        Node current;   //the current node
        open.Clear();   //clear the list
        closed.Clear(); //clear the list
        path.Clear();   //clear the path

        //add start to the list
        open.Add(new Node()
        {
            Pos = Start, //set the position to the start position
            G = 0, //set the cost to 0 as this is the start node
            H = GetHeuristicScore(Start) //get the heuristic and set it
        });

        //whilst there are nodes in the open list
        while (open.Count > 0)
        {
            //get the current node
            current = open[0];

            //get the node with the smallest F-cost
            for (int i = 1; i < open.Count; i++)
            {
                if (open[i].F < current.F)
                    current = open[i];
            }

            //add the current node to the closed list and remove it from the open list
            closed.Add(current);
            open.Remove(current);

            //if we reached the target, exit
            if (current.Pos == Target)
                break;

            List<Position> walkableTiles = GetWalkableTiles(current.Pos); //get the walkable positions around the current position
            g++; //increment the cost by 1


            //loop through the walkable positions
            foreach (Position walkablePosition in walkableTiles)
            {

                //if the node has already been calculated, ignore it
                if (closed.FirstOrDefault(_node => _node.Pos == walkablePosition) != null)
                    continue;

                //try get the first node that matches the position
                Node? walkableNode = open.FirstOrDefault(_node => _node.Pos == walkablePosition);

                //if the node hasn't been calculated yet
                if (walkableNode == null)
                {
                    Node newNode = new()
                    {
                        Pos = walkablePosition, //set the node's position to this
                        G = g, //set the cost to the required cost
                        H = GetHeuristicScore(walkablePosition), //get the heuristic score
                        ParentNode = current,
                    };

                    //insert it in the non-calculated list at position 0
                    open.Insert(0, newNode);
                }
                //if the node has previously been calculated
                else
                {
                    //if the current F-score is lower than the node's previous F-score
                    if (g + walkableNode.H < walkableNode.F)
                    {
                        walkableNode.G = g;
                        walkableNode.ParentNode = current;
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private List<Position> GetWalkableTiles(Position _current)
    {
        //get the proposed positions
        List<Position> proposed = new() {
            _current + Position.Up,
            _current + Position.Down,
            _current + Position.Left,
            _current + Position.Right,
        };

        //use a predicate to filter for the walkable tiles
        return proposed.FindAll(_pos => World.IsWall(_pos) == false);
    }

    private int GetHeuristicScore(Position _pos)
    {
        int h = Math.Abs(_pos.X - Target.X + _pos.Y - Target.Y);

        //FOR DEBUGGING
        //if (Debugger.IsAttached)
        //{
        //    Thread.Sleep(30);
        //    Display.DebugMark(_pos.X, _pos.Y, ConsoleColor.Blue);
        //}

        return h;
    }
}
