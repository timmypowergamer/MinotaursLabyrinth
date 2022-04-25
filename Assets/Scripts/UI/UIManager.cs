using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

	[SerializeField] private GameObject HudGroup;
	[SerializeField] private GameObject MenuGroup;
	[SerializeField] private GameObject CongratsText;
	[SerializeField] private Toggle autoSolveToggle;
	[SerializeField] private GameObject ErrorDialog;
	[SerializeField] private TMPro.TextMeshProUGUI errorText;

	[SerializeField] private TextAsset[] mazes;

	private void Awake()
	{
        Instance = this;
		CongratsText.SetActive(false);
		ShowHUD(false);
		ShowMenu(true);
	}

	public void MoveDirectionPressed(int direction)
	{
		MazeController.Instance.Character.MoveDirection(direction);
	}

	public void ToggleAutoSolvePressed()
	{
		MazeController.Instance.Character.ToggleAutoSolve();
	}

	public void RetryPressed()
	{
		CongratsText.SetActive(false);
		MazeController.Instance.LoadMaze();
	}

	public void BackToMenuPressed()
	{
		CongratsText.SetActive(false);
		MazeController.Instance.ClearMaze();
		ShowHUD(false);
		ShowMenu(true);
	}

	public void ToggleDepthDisplayPressed(bool isOn)
	{
		MazeController.Instance.ShowDepthOverlay = isOn;
	}

	private void Update()
	{
		if(MazeController.Instance.Character != null)
		{
			if(MazeController.Instance.Character.IsSolving != autoSolveToggle.isOn)
			{
				autoSolveToggle.SetIsOnWithoutNotify(MazeController.Instance.Character.IsSolving);
			}
		}
	}

	public void ShowCongratsMessage()
	{
		CongratsText.SetActive(true);
	}

	public void ShowHUD(bool value)
	{
		HudGroup.SetActive(value);
	}
	public void ShowMenu(bool value)
	{
		MenuGroup.SetActive(value);
	}

	public void LoadCustomMaze()
	{
		FileBrowser.SetFilters(true, ".txt");
		FileBrowser.SetDefaultFilter(".txt");
		FileBrowser.ShowLoadDialog(onLoadSuccess, ()=> { }, FileBrowser.PickMode.Files, title: "Pick ascii maze file");
	}

	private void onLoadSuccess(string[] paths)
	{
		MazeController.Instance.LoadMaze(FileBrowserHelpers.ReadTextFromFile(paths[0]));
	}

	public void LoadMaze(int mazeIdx)
	{
		MazeController.Instance.LoadMaze(mazes[mazeIdx].text);
	}

	public void ShowErrorBox(string error)
	{
		errorText.text = error;
		ErrorDialog.SetActive(true);
	}

	public void HideErrorBox()
	{
		ErrorDialog.SetActive(false);
	}

	public void QuitGamePressed()
	{
		Application.Quit();
	}
}
