using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    private Tower _placedTower;

    //fungsi terpanggil 1x ketika ada object Rigidbody yg menyentuh area collider
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(_placedTower != null)
        {
            return;
        }

        Tower tower = collision.GetComponent<Tower>();
        if(tower != null)
        {
            tower.SetPlacePosition(transform.position);
            _placedTower = tower;
        }
    }

    //fungsi terpanggil ketika object Rigidbody meninggalkan area collider
    private void OnTriggerExit2D(Collider2D collision) 
    {
        if(_placedTower == null)
        {
            return;
        }

        _placedTower.SetPlacePosition(null);
        _placedTower = null;
    }


}//class
