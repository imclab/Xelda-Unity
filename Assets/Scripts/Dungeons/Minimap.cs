using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


public class Minimap : FContainer
{
	private string visitedPNG = "minimap_visited";
	private string unvisitedPNG = "minimap_unvisited";
	
	private float originX;
	private float originY;
	
	private float nodeWidth;
	private float nodeHeight;
	
	private MinimapNode playerNode;
	
	private List<MinimapNode> nodeList;
	
	private class MinimapNode : FSprite
	{	
		public Vector2 coordinates;
		public bool Visited;
		
		// for map nodes
		public MinimapNode(string name, Vector2 coords, float x, float y) : base (name + ".png")
		{
			this.x = x;
			this.y = y;
			this.coordinates = coords;
			this.Visited = false;
			this.isVisible = false;
		}
		
		// for player node
		public MinimapNode(string name, float x, float y) : base (name + ".png")
		{
			this.x = x;
			this.y = y;
		}
	}
	
	public Minimap (Dungeon dungeon)
	{
		FSprite visitedNode = new FSprite(visitedPNG + ".png");
		nodeWidth = visitedNode.width;
		nodeHeight = visitedNode.height;
		
		nodeList = new List<MinimapNode>();
		
		// position this in top left of screen
		this.x = -Futile.screen.halfWidth + nodeWidth;
		this.y = Futile.screen.halfHeight;
		
		// set origin to bottom, left corner
		originX = 0;
		originY = -(dungeon.maxHeight * nodeHeight) - (nodeHeight / 2);
		
		AddMapNodes(dungeon.RoomList, dungeon.CurrentRoom);
	}
	
	public void AddMapNodes(List<Room> roomList, Room currentRoom)
	{	
		// debug draw entire possible rooms in dungeon
//		for(int i = 0; i < 8; i++)
//		{
//			for (int j = 0; j < 8; j++)
//			{
//				AddChild(
//					new MinimapNode(
//						"minimap_visited", new Vector2(0,0), originX + (nodeWidth * i), originY + (nodeHeight * j)
//					)
//				);
//			}
//		}
		
		// add all nodes to list and screen
		foreach(Room r in roomList)
		{
			string nodeName = (r.Visited) ? visitedPNG : unvisitedPNG;
			Vector2 pos = CalculateMinimapNodePosition(r);
			MinimapNode node = new MinimapNode(nodeName, r.MinimapRoomCoordinates, pos.x, pos.y);
			if (r.Visited)
			{
				node.Visited = true;
				node.isVisible = true;
			}
			nodeList.Add(node);
			AddChild(node);
		}
		
		MakeAdjacentNodesVisible(roomList);
		
		//Add initial player node
		Vector2 player_pos = CalculateMinimapNodePosition(currentRoom);
		playerNode = new MinimapNode("minimap_marker", player_pos.x, player_pos.y);
		AddChildAtIndex(playerNode,99);
	}
	
	private void MakeAdjacentNodesVisible(List<Room> roomList)
	{
		// set unvisited but connected to visited nodes as visible
		foreach(MinimapNode n in nodeList)
		{
			if (n.Visited)
			{
				List<Direction> dirList = roomList.Find(r => r.MinimapRoomCoordinates == n.coordinates).GetConnectedDirections();
				foreach(Direction d in dirList)
				{
					Vector2 tempV = Vector2.zero;
					switch(d)
					{
					case Direction.N:
						tempV = new Vector2(n.coordinates.x, n.coordinates.y + 1f);
						break;
					case Direction.S:
						tempV = new Vector2(n.coordinates.x, n.coordinates.y - 1f);
						break;
					case Direction.W:
						tempV = new Vector2(n.coordinates.x - 1, n.coordinates.y);
						break;
					case Direction.E:
						tempV = new Vector2(n.coordinates.x + 1, n.coordinates.y);
						break;
					}
					nodeList.Find(node => node.coordinates == tempV).isVisible = true;
				}
			}
		}
	}
	
	public void UpdateMinimap(Dungeon dungeon)
	{
		// remove old node
		dungeon.CurrentRoom.Visited = true;
		MinimapNode currentNode = nodeList.First(x => x.coordinates == dungeon.CurrentRoom.MinimapRoomCoordinates);
		RemoveChild(currentNode);
		nodeList.Remove(currentNode);
		
		// add new node with updated sprite
		Vector2 pos = CalculateMinimapNodePosition(dungeon.CurrentRoom);
		MinimapNode newNode = new MinimapNode(visitedPNG, dungeon.CurrentRoom.MinimapRoomCoordinates,pos.x, pos.y);
		newNode.isVisible = true;
		newNode.Visited = true;
		AddChild(newNode);
		nodeList.Add(newNode);
		
		// show the adjacent unvisited nodes
		MakeAdjacentNodesVisible(dungeon.RoomList);
		
		this.RemoveChild(playerNode);
		AddChildAtIndex(playerNode,99);
	}
	
	public Vector2 CalculateMinimapNodePosition(Room room)
	{	
		return new Vector2(
			originX + (nodeWidth  * room.MinimapRoomCoordinates.x), 
			originY + (nodeHeight * room.MinimapRoomCoordinates.y)
		);
	}
	
	public void ChangePlayerIconPosition(Direction dir)
	{
		switch(dir)
		{
		case (Direction.N):
			playerNode.y += 12;
			break;
		case (Direction.S):
			playerNode.y -= 12;
			break;
		case (Direction.W):
			playerNode.x -= 24;
			break;
		case (Direction.E):
			playerNode.x += 24;
			break;
		}
	}
}