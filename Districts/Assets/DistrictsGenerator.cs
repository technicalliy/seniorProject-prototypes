﻿using UnityEngine;
using System.Collections;
using AssemblyCSharp;


public class DistrictsGenerator : MonoBehaviour {

	[SerializeField]
	private Vector2[] districtEndPoints;

	[SerializeField]
	private Vector2 cityCenter;

	[SerializeField]
	private District[] districts;

	/// <summary>
	/// Entry point of the generator
	/// </summary>
	void Start () {
		generateDistrictPoints (3, 500);
	}

	/// <summary>
	/// Generates city center and district edges. Edges of districts are defined as (cityCenter)->(districtEndPoints[i])
	/// </summary>
	/// <param name="numDistricts">Number of districts desired -- currently supports 3.</param>
	/// <param name="districtMaxSpan">District max span for one side from center point to edge.</param>
	void generateDistrictPoints(int numDistricts, int districtMaxSpan){

		Vector2[] initialSeedPoints = new Vector2[numDistricts];
		Vector2[] seedMidPoints		= new Vector2[numDistricts];

				  districtEndPoints = new Vector2[numDistricts];
				  cityCenter 	    = new Vector2();
			      districts 		= new AssemblyCSharp.District[numDistricts];

		// generate initial random seed points
		for (int i = 0; i < numDistricts; ++i) {
			initialSeedPoints[i].x = Random.Range(1, districtMaxSpan);
			initialSeedPoints[i].y = Random.Range(1, districtMaxSpan);
		}

		// calculate midpoints of all initial seeds
		for (int i = 0; i < numDistricts; ++i){
			for (int j = 0; j  < numDistricts; ++j){
				float x = (initialSeedPoints[i].x+initialSeedPoints[j].x)/2;
				float y = (initialSeedPoints[i].y+initialSeedPoints[j].y)/2;
				seedMidPoints[j].x = x;
				seedMidPoints[j].y = y;
			}
		}

		// using these midpoints, get the center of the city
		float cityCenterX = 0;
		float cityCenterY = 0;

		for (int i = 0; i < numDistricts; ++i){
			cityCenterX += seedMidPoints[i].x;
			cityCenterY += seedMidPoints[i].y;
		}

		cityCenter.x = cityCenterX/numDistricts;
		cityCenter.y = cityCenterY/numDistricts;

		// for each point, get the slope, its length based districtSpan, and its position relative to city center
		for (int i = 0; i < numDistricts; ++i){

			float slope = ((float)(cityCenterY-seedMidPoints[i].y))/((float)(cityCenterX-seedMidPoints[i].x));
			float k = districtMaxSpan/(Mathf.Sqrt(1+Mathf.Pow(slope, 2.0f)));

			// account for position of point relative in space when assigning end point
			float currentEndpointX = 0.0f;
			float currentEndpointY = 0.0f;

			if (seedMidPoints[i].x < cityCenterX)	currentEndpointX = (float)cityCenter.x - k;
			else 									currentEndpointX = (float)cityCenter.x + k;
		
			if (seedMidPoints[i].y < cityCenterY)	currentEndpointY = (float)cityCenter.y - (k*slope);
			else 									currentEndpointY = (float)cityCenter.y + (k*slope);

			districtEndPoints [i].x = currentEndpointX;
			districtEndPoints [i].y = currentEndpointY;

		}

		// assign each district its first two verticies based on the endpoints of its edges
		for (int i = 0; i < numDistricts; i++) {
			districts [i] = new District (cityCenter);

			Vector2[] districtPoints = new Vector2[2];

			// if it's the last district, its edges are that of the first and the last districts
			if (i == numDistricts-1){
				districtPoints[0] = districtEndPoints[districtEndPoints.Length-1];
				districtPoints[1] = districtEndPoints[0];
			} else {
				districtPoints[0] = districtEndPoints[i];
				districtPoints[1] = districtEndPoints[i+1];
			}

			districts [i].setVerticies (districtPoints);
		}

		// generate verticies for each district to allow for more natural edges
		generateCityEdges (50, 100, 800, 300);
	}

	/// <summary>
	/// Generates the city edge vertices.
	/// </summary>
	/// <param name="minVerts">Minimum # of vertices for edges of city.</param>
	/// <param name="maxVerts">Maximum # of vertices for edges of city</param>
	/// <param name="maxDistFromCenter">Maximum distance of all verticies from center of city.</param>
	/// <param name="minDistFromCenter">Minimum distance of all verticies from center of city.</param>
	void generateCityEdges(int minVerts, int maxVerts, float maxDistFromCenter, float minDistFromCenter){

		// for each district, generate random # of randomly angled points within the current area of the district
		for (int i = 0; i < districts.Length; ++i){

			int numVerts = Random.Range (minVerts, maxVerts);
			Vector2[] newDistrictVerts = new Vector2 [numVerts+2];	// must allow for two initial points
			Vector2[] currentDistrictVerts = districts [i].getVerticies();

			// add initial district edges to list of verticies
			newDistrictVerts [0] = currentDistrictVerts [0];
			newDistrictVerts [1] = currentDistrictVerts [1]; 

			for (int j = 0; j < numVerts; ++j){

				// determine distance of point from center of city using perlin noise
				float percentageLength = Mathf.PerlinNoise (cityCenter.x, cityCenter.y);
				float distanceFromCenter = percentageLength * maxDistFromCenter;

				// compute a random point within the district to use as reference for angle from center
				float newX = Random.Range (currentDistrictVerts [0].x, currentDistrictVerts [1].x);
				float newY = Random.Range (currentDistrictVerts [0].y, currentDistrictVerts [1].y);
				Vector2 newRayPoint = new Vector2 (newX, newY);

				// new district vert is endpoint of line extending from city center through new random point at perlin length
				float angleFromCenter = Vector2.Angle (cityCenter, newRayPoint);
				newDistrictVerts [j].x = cityCenter.x + (distanceFromCenter) * Mathf.Cos (angleFromCenter);
				newDistrictVerts [j].y = cityCenter.y + (distanceFromCenter) * Mathf.Sin (angleFromCenter);

			}

			districts [i].setVerticies (newDistrictVerts);
		}

		/* This is for testing purposes */
		bool[,] pointsCheck = new bool[100,3];
		for (int i = 0; i < 100; i++) {

			float x = Random.Range (-500, 500);
			float y = Random.Range (-500, 500);

			Vector2 newPoint = new Vector2 (x, y);

			for (int j = 0; j < 3; j++){
				pointsCheck[i,j] = districts[j].containsPoint(newPoint);
			}
		}
	}
}


// http://stackoverflow.com/questions/1638437/given-an-angle-and-length-how-do-i-calculate-the-coordinates