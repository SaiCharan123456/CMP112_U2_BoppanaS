using UnityEngine;
using System.Collections.Generic;
public class SoundWaveBullet : MonoBehaviour 
{ 

    [Header("Wave Settings")]
    public float maxRadius = 10f; 
    public float expandSpeed = 8f; 
    public float moveSpeed = 10f; 
    public float damage = 100f; 
    public LayerMask ghostLayer; 
    private float currentRadius = 0f; 
    private HashSet<IDamageable> hitGhosts = new HashSet<IDamageable>(); 
    private Vector3 direction = Vector3.forward; 
    
    [Header("Height Settings")] 
    public float thickness = 0.5f;    // vertical thickness of the 2D circle
    
    [Header("Visual FX")] 
    [SerializeField] private ParticleSystem soundWaveFX; 
    
    public void SetDirection(Vector3 dir) { 
        direction = dir.normalized; 
    } 
    void Update() {
        // Move bullet forward
        transform.position += direction * moveSpeed * Time.deltaTime; 
        
        // Expand 2D radius on XZ plane
       
        currentRadius += expandSpeed * Time.deltaTime; 
        DetectGhosts(); 
        
        // Scale visual FX
        if (soundWaveFX != null) { 
            soundWaveFX.Play(); 
            soundWaveFX.transform.localScale = new Vector3(currentRadius * 2f, currentRadius * 2f, 1f); 
        } 
        
        if (currentRadius >= maxRadius) { 
            Destroy(gameObject); 
        } 
    } 
    void DetectGhosts() { 
        // OverlapBox as a thin cylinder approximation for 2D circle
        Vector3 boxCenter = transform.position; 
        Vector3 boxHalfExtents = new Vector3(currentRadius, currentRadius, thickness / 2f); 

        Collider[] hits = Physics.OverlapBox(boxCenter, boxHalfExtents, Quaternion.identity, ghostLayer); 
        
        foreach (Collider col in hits) { 
            IDamageable ghost = col.GetComponentInParent<IDamageable>(); 
            if (ghost != null && !hitGhosts.Contains(ghost)) { 
                // Only affect ghosts within true XY circle
                Vector3 flatDelta = col.transform.position - transform.position; 
                flatDelta.z = 0; 
                // ignore Z for XY plane
                if (flatDelta.magnitude <= currentRadius) {
                    ghost.TakeDamage(damage); 
                    hitGhosts.Add(ghost); 
                } 
            } 
        } 
    } 
    
    // For visualization in editor
    
    private void OnDrawGizmosSelected() { 
        Debug.DrawLine(transform.position,transform.position + new Vector3(currentRadius, 0, 0),Color.green );
        Debug.DrawLine(transform.position,transform.position + new Vector3(0, currentRadius, 0),Color.blue);

    }
}