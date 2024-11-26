using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;
    [SerializeField] float finalDistance;
    [SerializeField] float sideRayDistance;
    [SerializeField] float distanceFromHit;
    [SerializeField] float distanceFromSideHit;
    [SerializeField] float smooth;

    [Header("Private References")]
    Vector3 cameraDir;

    // Start is called before the first frame update
    void Awake()
    {
        cameraDir = transform.localPosition.normalized;
        finalDistance = maxDistance;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // la distancia estandar a la que se situará la cámara
        Vector3 desiredCameraPos = transform.parent.TransformPoint(cameraDir * maxDistance);
        
        RaycastHit backHit;
        RaycastHit sideHit;

        //Linecast principal entre la distancia mínima a la cual puede estar la cámara y la distancia máxima, sale desde la dirección contraria de la cámara y detecta obstáculos
        if (Physics.Linecast(transform.parent.position, desiredCameraPos, out backHit))
        {
            finalDistance = Mathf.Clamp(backHit.distance * distanceFromHit, minDistance, maxDistance);
        }
        else
        {
            // Si no hay ningún obstáculo, la distancia final será la máxima
            finalDistance = maxDistance;

            for (int i = 0; i < 4; i++)
            {
                // Si el Linecast no ha detectado haremos una comprobación con 4 Raycast diferentes que salen en oblicuo desde lel mismo sitio

                // Se definen cuatro direcciones en las que saldrá un Raycast oblicuo para evitar que la cámara entre en algunos objetos 

                int sideRayAngle = 30; // Ángulo en el cual los Raycast saldrán respecto a la dirección contraria de la cámara

                Vector3[] sideSelect;
                sideSelect = new Vector3[4];

                sideSelect[0] = Quaternion.Euler(sideRayAngle, 0, 0) * Vector3.back;
                sideSelect[1] = Quaternion.Euler(-sideRayAngle, 0, 0) * Vector3.back;
                sideSelect[2] = Quaternion.Euler(0, sideRayAngle, 0) * Vector3.back;
                sideSelect[3] = Quaternion.Euler(0, -sideRayAngle, 0) * Vector3.back;

                if (Physics.Raycast(transform.position, sideSelect[i], out sideHit, sideRayDistance))
                {
                    finalDistance = maxDistance - distanceFromSideHit;

                    // Sale del bucle si encuentra una colisión
                    break;
                }
            }
        }

        // Ajustamos la posición final tomando como Vector la dirección a la que tiene tiene que moverse la cámara siempre y la distancia calculada
        Vector3 finalPosition = cameraDir * finalDistance;

        // Se aplica el movimiento con la posición final obtenida aplicando un ajuste para suavizar la velocidad a la que se aproxima la cámara
        transform.localPosition = Vector3.Lerp (transform.localPosition, finalPosition, smooth * Time.deltaTime);
    }
}
