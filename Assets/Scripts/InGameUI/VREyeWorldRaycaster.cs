using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VREyeWorldRaycaster : Singleton<VREyeWorldRaycaster>
{
	[Tooltip("A world space pointer for this canvas")]
	public GameObject pointer;

	[SerializeField]
	protected float maxDistance = 50f;

	void FixedUpdate()
	{
		RaycastHit hit;

		// do a forward raycast to see if we hit a Button
		if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
		{
			pointer.transform.position = hit.point;
		}
	}
}
