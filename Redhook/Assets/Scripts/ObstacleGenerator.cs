using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleGenerator : MonoBehaviour
{
	public Transform ObstaclesParent;
	public CellGrid CellGrid;
	private bool done;
	private IEnumerator spawnObstacles;

	public void Start()
	{
		done = false;
		spawnObstacles = SpawnObstacles();
		StartCoroutine(spawnObstacles);
	}

	void Update()
	{
		if (done)
		{
			Debug.Log("stop");
			StopCoroutine(spawnObstacles);
			done = false;
		}
	}

	/// <summary>
	/// Method sets position on obstacle objects and 
	/// sets isTaken field on cells that the obstacles are occupying
	/// </summary>
	public IEnumerator SpawnObstacles()
	{
		while (CellGrid.Cells == null)
		{
			yield return 0;
		}
		var cells = CellGrid.Cells;

		for (int i = 0; i < ObstaclesParent.childCount; i++)
		{
			Debug.Log("hmm");
			var obstacle = ObstaclesParent.GetChild(i);
			var bounds = getBounds(obstacle);

			IEnumerable<Cell> closest = cells.OrderBy(h => Math.Abs((h.transform.position - obstacle.transform.position).magnitude));
			foreach (Cell cell in closest)
			{
				if (bounds.Contains(cell.GetComponent<Renderer>().bounds.center + Vector3.up * bounds.size.y / 2))
				//if (bounds.Intersects(cell.GetComponent<Renderer>().bounds))
				{
					cell.IsTaken = true;
					cell.Occupier.Add(obstacle.gameObject);
				}
			}
		}
		done = true;
	}

	/// <summary>
	/// Method snaps obstacle objects to the nearest cell.
	/// </summary>
	public void SnapToGrid()
	{
		List<Transform> cells = new List<Transform>();

		foreach (Transform cell in CellGrid.transform)
		{
			cells.Add(cell);
		}

		foreach (Transform obstacle in ObstaclesParent)
		{
			var bounds = getBounds(obstacle);
			var closestCell = cells.OrderBy(h => Math.Abs((h.transform.position - obstacle.transform.position).magnitude)).First();
			if (!closestCell.GetComponent<Cell>().IsTaken)
			{
				Vector3 offset = new Vector3(0, bounds.size.y, 0);
				obstacle.localPosition = closestCell.transform.localPosition + offset;
			}
		}
	}

	private Bounds getBounds(Transform transform)
	{
		var renderer = transform.GetComponent<Renderer>();
		if (renderer == null) {
			renderer = transform.GetChild(0).GetComponent<Renderer>();
		}
		var combinedBounds = renderer.bounds;
		var renderers = transform.GetComponentsInChildren<Renderer>();
		foreach (var childRenderer in renderers)
		{
			if (childRenderer != renderer) combinedBounds.Encapsulate(childRenderer.bounds);
		}

		return combinedBounds;
	}
}

