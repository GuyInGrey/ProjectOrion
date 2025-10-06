using NUnit.Framework;

using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public List<Image> bookIngredientImages;
    public TMP_Text bookPotNameText;
    public Image finalPotLiquidImage;
    public Cauldron cauldron;
    public Button leftButton;
    public Button rightButton;
    public RectTransform bookTransform;

    int currentPage = 0;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Shop")
        {
            LoadPage();
        }
    }

    public void Dungeon_ShopButtonPressed()
    {
        SceneManager.LoadSceneAsync("Shop", LoadSceneMode.Single);
    }

    public void Shop_DungeonPressed()
    {
        SceneManager.LoadSceneAsync("Dungeon", LoadSceneMode.Single);
    }

    public void DirectionPressed(bool isLeft)
    {
        currentPage += isLeft ? -1 : 1;
        LoadPage();
    }

    void LoadPage()
    {
        leftButton.interactable = currentPage > 0;
        rightButton.interactable = currentPage < cauldron.craftablePotionTypes.Count - 1;

        var potType = cauldron.craftablePotionTypes[currentPage];
        bookPotNameText.text = potType.potionName;
        finalPotLiquidImage.color = potType.color;

        for (var i = 0; i < bookIngredientImages.Count; i++)
        {
            var ing = i >= potType.ingredients.Count ? null : potType.ingredients[i];
            bookIngredientImages[i].sprite = ing == null ? null : ing.sprite;
            bookIngredientImages[i].color = ing == null ? new Color(1, 1, 1, 0) : Color.white;
        }
    }

    public void ToggleBook()
    {
        bookTransform.gameObject.SetActive(!bookTransform.gameObject.activeSelf);
    }

    public void Title_PlayPressed()
    {
        SceneManager.LoadSceneAsync("Shop", LoadSceneMode.Single);
    }

    public void Title_QuitPressed()
    {
        Application.Quit();
    }
}
