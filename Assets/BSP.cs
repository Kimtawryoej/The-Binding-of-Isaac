using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BSP : MonoBehaviour
{
    [SerializeField] Vector2Int mapSize; //만들고 싶은 맵의 크기
    [SerializeField] private LineRenderer map;
    [SerializeField] private LineRenderer line;
    [SerializeField] private LineRenderer room;
    [SerializeField] private LineRenderer conncetLine;
    [SerializeField] private float minimumDevideRate;
    [SerializeField] private float maximumDivideRate;
    [SerializeField] private int maximumDepth;

    void Start()
    {
        DrawMap(0, 0);
        Node root = new(new RectInt(0, 0, mapSize.x, mapSize.y));
        Divide(root, 0);
        CreatRoom(root, 0);
        ConnectLine(root, 0);
    }

    private void DrawMap(int x, int y)
    {
        LineRenderer lineRenderer = Instantiate(map);
        lineRenderer.SetPosition(0, new Vector2(x, y) - mapSize / 2); //좌측 하단
        lineRenderer.SetPosition(1, new Vector2(x + mapSize.x, y) - mapSize / 2); //우측 하단
        lineRenderer.SetPosition(2, new Vector2(x + mapSize.x, y + mapSize.y) - mapSize / 2);//우측 상단
        lineRenderer.SetPosition(3, new Vector2(x, y + mapSize.y) - mapSize / 2); //좌측 상단
    }

    private void Divide(Node node, int count)
    {
        if (count == maximumDepth) { return; }

        int maxLength = Mathf.Max(node.NodeRect.width, node.NodeRect.height);
        int DevideRate = Mathf.RoundToInt(Random.Range(maxLength * minimumDevideRate, maxLength * maximumDivideRate));
        if (node.NodeRect.width >= node.NodeRect.height)
        {
            AddNode(new RectInt(node.NodeRect.x, node.NodeRect.y, DevideRate, node.NodeRect.height),
                    new RectInt(node.NodeRect.x + DevideRate, node.NodeRect.y, node.NodeRect.width - DevideRate, node.NodeRect.height));
            //DrawLine(new Vector2Int(node.NodeRect.x + DevideRate, node.NodeRect.height + node.NodeRect.y), new Vector2Int(node.NodeRect.x + DevideRate, node.NodeRect.y));
        }
        else
        {
            AddNode(new RectInt(node.NodeRect.x, node.NodeRect.y + DevideRate, node.NodeRect.width, node.NodeRect.height - DevideRate),
                    new RectInt(node.NodeRect.x, node.NodeRect.y, node.NodeRect.width, DevideRate)) ;
            //DrawLine(new Vector2Int(node.NodeRect.x, node.NodeRect.y + DevideRate), new Vector2Int(node.NodeRect.x + node.NodeRect.width, node.NodeRect.y + DevideRate));
        }

        node.LeftNode.ParNode = node;
        node.RightNode.ParNode = node;
        Divide(node.LeftNode, count + 1);
        Divide(node.RightNode, count + 1);
        void AddNode(RectInt left, RectInt right)
        {
            node.LeftNode = new(left);
            node.RightNode = new(right);
        }
        void DrawLine(Vector2Int pos1, Vector2Int pos2)
        {
            LineRenderer lineRenderer = Instantiate(line);
            lineRenderer.SetPosition(0, new Vector2(pos1.x, pos1.y) - mapSize / 2);
            lineRenderer.SetPosition(1, new Vector2(pos2.x, pos2.y) - mapSize / 2);
        }
    }

    RectInt CreatRoom(Node node, int count)
    {
        RectInt roomRect = new RectInt();
        if (count == maximumDepth)
        {
            roomRect = node.NodeRect;
            int height = Random.Range(roomRect.height / 2, roomRect.height - 1);
            int width = Random.Range(roomRect.width / 2, roomRect.width - 1);
            int x = roomRect.x + Random.Range(1, roomRect.width - width);
            int y = roomRect.y + Random.Range(1, roomRect.height - height);
            roomRect = new RectInt(x, y, width, height);
            DrawRoom(roomRect);
        }
        else
        {
            node.LeftNode.RoomRect = CreatRoom(node.LeftNode, count + 1);
            node.RightNode.RoomRect = CreatRoom(node.RightNode, count + 1);
            roomRect = node.LeftNode.RoomRect;
        }

        void DrawRoom(RectInt roomPos)
        {
            LineRenderer lineRenderer = Instantiate(room);
            lineRenderer.SetPosition(0, new Vector2(roomPos.x, roomPos.y) - mapSize / 2);
            lineRenderer.SetPosition(1, new Vector2(roomPos.x + roomPos.width, roomPos.y) - mapSize / 2);
            lineRenderer.SetPosition(2, new Vector2(roomPos.x + roomPos.width, roomPos.y + roomPos.height) - mapSize / 2);
            lineRenderer.SetPosition(3, new Vector2(roomPos.x, roomPos.y + roomPos.height) - mapSize / 2);

        }
        return roomRect;
    }

    void ConnectLine(Node node, int count)
    {
        //Debug.Log(count);
        if (count == maximumDepth) return;

        Debug.Log(node.NodeRect.center);
        Vector2Int leftNodeCenter = (count == maximumDepth - 1) ? node.LeftNode.RoomCenter : node.LeftNode.Center;
        Vector2Int rightNodeCenter = (count == maximumDepth - 1) ? node.RightNode.RoomCenter : node.RightNode.Center; ;
        DrawConnectLine(new Vector2(node.Center.x, node.Center.y), new Vector2(leftNodeCenter.x, leftNodeCenter.y));
        DrawConnectLine(new Vector2(node.Center.x, node.Center.y), new Vector2(rightNodeCenter.x, rightNodeCenter.y));
        ConnectLine(node.LeftNode, count + 1);
        ConnectLine(node.RightNode, count + 1);
        //DrawConnectLine(new Vector2(leftNodeCenter.x, leftNodeCenter.y), new Vector2(rightNodeCenter.x, leftNodeCenter.y));
        //DrawConnectLine(new Vector2(rightNodeCenter.x, leftNodeCenter.y), new Vector2(rightNodeCenter.x, rightNodeCenter.y));
        //else if (count == maximumDepth - 1)
        //{
        //}
        //else
        //{
        //    ConnectLine(node.leftNode, count + 1);
        //    ConnectLine(node.rightNode, count + 1);
        //}

        void DrawConnectLine(Vector2 nodeRoom, Vector2 room)
        {
            LineRenderer lineRendererLeft = Instantiate(conncetLine);
            lineRendererLeft.SetPosition(0, new Vector2(nodeRoom.x, nodeRoom.y) - mapSize / 2);
            lineRendererLeft.SetPosition(1, new Vector2(room.x, room.y + 1) - mapSize / 2);
        }
    }
}

public class Node
{
    public Node LeftNode;
    public Node RightNode;
    public Node ParNode;
    public RectInt NodeRect; //분리된 공간의 rect정보
    public RectInt RoomRect; //분리된 공간의 room rect정보

    public Vector2Int Center
    {
        get
        {
            return new Vector2Int(NodeRect.x + NodeRect.width / 2, NodeRect.y + NodeRect.height / 2);
        }
        //방의 가운데 점. 방과 방을 이을 때 사용
    }

    public Vector2Int RoomCenter
    {
        get
        {
            return new Vector2Int(RoomRect.x + RoomRect.width / 2, RoomRect.y + RoomRect.height / 2);
        }
        //방의 가운데 점. 방과 방을 이을 때 사용
    }
    public Node(RectInt rect)
    {
        this.NodeRect = rect;
    }
}



