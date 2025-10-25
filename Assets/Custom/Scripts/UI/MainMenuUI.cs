using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
   public GameObject mainMenu;
   public GameObject optionsMenu;
   public void Start()
   {
    ShowMainMenu();
   }

   public void ShowMainMenu()
   {
    mainMenu.SetActive(true);
    optionsMenu.SetActive(false);
   }
   public void ShowOptionsMenu()
   {
    mainMenu.SetActive(false);
    optionsMenu.SetActive(true);               
   }

}
