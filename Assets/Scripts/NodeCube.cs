using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace IKSpider.Orientation
{
    public enum TypeOfPrecedingCube
    {
        Forward, Backward, Up, Down, Left, Right, None
    }

    public enum StateOfCube
    {
        Collided = 0, NotCollided = 1, NotVisited = 2,
    }

    public class NodeCube
    {
        private Transform _transform;
        private Vector3 _position;
        private float _size;
        private float _distance;

        // neighbour cubes
        private NodeCube _right;
        private NodeCube _left;
        private NodeCube _up;
        private NodeCube _down;
        private NodeCube _forward;
        private NodeCube _backward;

        private NodeCube[] _neighbourCubes;

        // world position of the cube
        public Vector3 Position => _position + _transform.position;

        public float Size => _size;
        public StateOfCube State => _stateOfCube;

        private List<NodeCube> _listOfNodes;

        private StateOfCube _stateOfCube = StateOfCube.NotVisited;

        private List<Vector3> _hitNormals = new List<Vector3>();
        public List<Vector3> HitNormals => _hitNormals;

        // for debug reasons
        private List<Vector3> _hitPoints = new List<Vector3>();
        public List<Vector3> HitPoints => _hitPoints;

        private NodeCube TryToInitNode(Vector3 incrementPos, TypeOfPrecedingCube typeOfPrecedingCube)
        {
            NodeCube nodeCube = null;
            Vector3 cubePos = _position + incrementPos * _size;
            float distanceFromCube = cubePos.magnitude;

            if (distanceFromCube < _distance)
            {
                NodeCube nearNode = _listOfNodes.
                    FirstOrDefault(a =>
                        {
                            if ((Mathf.Abs(a._position.x - cubePos.x) < (_size * 0.0001f)) &&
                                (Mathf.Abs(a._position.y - cubePos.y) < (_size * 0.0001f)) &&
                                (Mathf.Abs(a._position.z - cubePos.z) < (_size * 0.0001f)))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        });

                if (nearNode != null)
                {
                    nodeCube = nearNode;
                    nodeCube.SetNearNode(this, typeOfPrecedingCube);
                }
                else
                {
                    nodeCube = new NodeCube(
                                        this,
                                        typeOfPrecedingCube,
                                        _transform,
                                        cubePos,
                                        _size,
                                        _distance,
                                        ref _listOfNodes);
                }
            }

            return nodeCube;
        }

        private void SetNearNode(NodeCube cube, TypeOfPrecedingCube typeOfPrecedingCube)
        {
            SetPrecedingCube(cube, typeOfPrecedingCube);
        }

        private void SetPrecedingCube(NodeCube precedingCube, TypeOfPrecedingCube type)
        {
            switch (type)
            {
                case TypeOfPrecedingCube.Down:
                    _down = precedingCube;
                    break;
                case TypeOfPrecedingCube.Up:
                    _up = precedingCube;
                    break;
                case TypeOfPrecedingCube.Right:
                    _right = precedingCube;
                    break;
                case TypeOfPrecedingCube.Left:
                    _left = precedingCube;
                    break;
                case TypeOfPrecedingCube.Forward:
                    _forward = precedingCube;
                    break;
                case TypeOfPrecedingCube.Backward:
                    _backward = precedingCube;
                    break;
            }
        }

        private void InitNeighbourCubes()
        {
            if (_up == null)
            {
                _up = TryToInitNode(Vector3.up, TypeOfPrecedingCube.Down);
            }

            if (_down == null)
            {
                _down = TryToInitNode(Vector3.down, TypeOfPrecedingCube.Up);
            }

            if (_right == null)
            {
                _right = TryToInitNode(Vector3.right, TypeOfPrecedingCube.Left);
            }

            if (_left == null)
            {
                _left = TryToInitNode(Vector3.left, TypeOfPrecedingCube.Right);
            }

            if (_forward == null)
            {
                _forward = TryToInitNode(Vector3.forward, TypeOfPrecedingCube.Backward);
            }

            if (_backward == null)
            {
                _backward = TryToInitNode(Vector3.back, TypeOfPrecedingCube.Forward);
            }

            _neighbourCubes = new NodeCube[]{
            _up, _down, _right, _left, _forward, _backward,
        };
        }

        public void StartCheckingFromTheNode()
        {
            // for now i start from upper node of first node
            _up.CheckNode();
        }

        private void CheckNode()
        {
            if (_stateOfCube != StateOfCube.NotVisited) return;

            if (Physics.CheckBox(Position, Vector3.one * Size * 0.5f))
            {
                _stateOfCube = StateOfCube.Collided;
            }
            else
            {
                _stateOfCube = StateOfCube.NotCollided;

                _up?.CheckNode();
                _down?.CheckNode();
                _backward?.CheckNode();
                _forward?.CheckNode();
                _right?.CheckNode();
                _left?.CheckNode();
            }
        }

        public void GenerateNormals()
        {
            foreach (var cube in _neighbourCubes)
            {
                if (_stateOfCube == StateOfCube.Collided
                    && cube != null
                    && cube._stateOfCube == StateOfCube.NotCollided)
                {
                    // may change this later for sphere cast or linecast
                    Vector3 direction = (Position - cube.Position) * 1.5f;
                    if (Physics.BoxCast(
                        cube.Position,
                        Vector3.one * _size * 0.5f,
                        direction,
                        out RaycastHit hit,
                        Quaternion.identity,
                        _size * 1.5f))
                    {
                        _hitNormals.Add(hit.normal);
                        _hitPoints.Add(hit.point);
                    }
                }
            }
        }

        public NodeCube(NodeCube precedingCube,
            TypeOfPrecedingCube typeOfPrecedingCube,
            Transform transform,
            Vector3 position,
            float size,
            float distance,
            ref List<NodeCube> nodeCubes)
        {
            if (size == 0) return;

            if (nodeCubes == null)
            {
                nodeCubes = new List<NodeCube>();
            }

            _listOfNodes = nodeCubes;

            _transform = transform;
            _position = position;
            _size = size;
            _distance = distance;

            _listOfNodes.Add(this);

            if (precedingCube != null)
            {
                SetPrecedingCube(precedingCube, typeOfPrecedingCube);
            }

            InitNeighbourCubes();
        }

        public void Reset()
        {
            _stateOfCube = StateOfCube.NotVisited;
            _hitNormals.Clear();
            _hitPoints.Clear();
        }
    }
}