using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // ��𼭵� ���� �����ϰ� �̱��� ����
    public List<Item> items = new List<Item>(); // �÷��̾ ȹ���� ������ ����

    [Header("UI ����")]
    public GameObject inventoryUI; // �����Ϳ��� InventoryPanel�� �巡���ؼ� �־��� ĭ
    public Transform slotParent;      // �κ��丮 �г� (���Ե��� �θ� ������Ʈ) = ȭ�鿡 ���̴� ����
    private InventorySlot[] slots;    // ��� ���� ����Ʈ

    private bool isInventoryOpen = false; //���� �� �κ��丮 â ����

    void Awake() { Instance = this; }

    void Start()
    {
        // �θ� �ؿ� �ִ� ��� InventorySlot�� �迭�� ������, ������ ������ �°� ������ ä��
        slots = slotParent.GetComponentsInChildren<InventorySlot>(true);
        UpdateUI();
    }

    void Update()
    {
        // 'I' Ű�� ������ �κ��丮 ����
        if (Input.GetKeyDown(KeyCode.I) && !GameProgress.InputLocked)
        {
            ToggleInventory();
        }
    }

    // ������ �߰� �Լ�
    public void AddItem(Item newItem)
    {
        items.Add(newItem);
        Debug.Log(newItem.itemName + " ȹ��!");
        if (GameMessageUI.Instance != null)
        {
            GameMessageUI.Instance.ShowMessage(newItem.itemName + "��(��) ȹ���ߴ�.");
        }
        UpdateUI(); // �������� ���� ������ ȭ�� ����
    }

    // ȭ�� ���� �ٽ� ����
    public void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < items.Count) // ������ �ִ� ������ ���� �����̶��
            {
                slots[i].AddItem(items[i]); // �ش� ������ ǥ��
            }
            else
            {
                slots[i].ClearSlot(); // �������� �� ĭ ó��
            }
        }
    }

    public void RemoveItem(string targetName)
    {
        foreach (InventorySlot slot in slots) // ��� ������ �ϳ��� Ȯ��
        {
            // ������ �̸��� ���� ������� ������ �̸��� ���ٸ�
            if (slot.itemName == targetName)
            {
                slot.ClearSlot(); 
                return; // ã�Ƽ� �������� ����
            }
        }
    }

    // Ư�� �������� ������ �ִ��� Ȯ���ϴ� �Լ�
    public bool HasItem(string targetName)
    {
        return items.Exists(x => x.itemName == targetName);
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen; //���: �Է��� ���� ������ ���¸� �ݴ�� �ٲ�
        inventoryUI.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            // �κ��丮�� ������ ���콺 Ŀ���� �����Ӱ� ��
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // �κ��丮�� ������ �ٽ� ȭ�� �߾ӿ� ����
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
