using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 상단 메뉴 [Tools > 퍼즐 자동 설치] 를 누르면
// 씬에 퍼즐 관련 컴포넌트/UI/열쇠를 전부 자동으로 만들고 연결해주는 에디터 스크립트
public static class PuzzleAutoSetup
{
    const string FontPath = "Assets/GmarketSansTTFMedium SDF.asset";
    const string KeyPrefabPath = "Assets/key/Keys/Prefabs/SImple_03.prefab";
    const string KeyIconPath = "Assets/key_icon.png";
    const string FinalKeyItemPath = "Assets/FinalKey.asset";

    const string Code = "0429";          // 책/노트북 4자리 숫자
    const string FinalKeyName = "최종 열쇠"; // 최종 열쇠 아이템 이름

    static List<string> log = new List<string>();

    [MenuItem("Tools/퍼즐 자동 설치 (Puzzle Auto Setup)")]
    public static void Setup()
    {
        log = new List<string>();

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
        {
            EditorUtility.DisplayDialog("오류", "한글 폰트를 찾지 못했습니다:\n" + FontPath, "확인");
            return;
        }

        int layer = LayerMask.NameToLayer("Interactable");

        // ---------- 1. GameProgress ----------
        if (Object.FindFirstObjectByType<GameProgress>() == null)
        {
            GameObject gp = new GameObject("GameProgress");
            gp.AddComponent<GameProgress>();
            log.Add("GameProgress 오브젝트 생성");
        }

        // ---------- 2. memo / memo_1 ----------
        GameObject memo = FindInScene("memo");
        if (memo != null)
        {
            MemoInteract mi = GetOrAdd<MemoInteract>(memo);
            mi.memoType = MemoInteract.MemoType.RunMemo;
            mi.content = "run";
            MakeInteractable(memo, layer);
            log.Add("memo: MemoInteract(RunMemo) 설정");
        }
        else log.Add("[주의] memo 오브젝트를 찾지 못함");

        GameObject memo1 = FindInScene("memo1");
        if (memo1 == null) memo1 = FindInScene("memo_1");
        if (memo1 != null)
        {
            MemoInteract mi = GetOrAdd<MemoInteract>(memo1);
            mi.memoType = MemoInteract.MemoType.HintMemo;
            mi.content = "잊어버리면 안 되는 것.\n그 서류의 답은...\n노트북 화면이 알고 있다.";
            MakeInteractable(memo1, layer);
            log.Add("memo_1: MemoInteract(HintMemo) 설정");
        }
        else log.Add("[주의] memo1 / memo_1 오브젝트를 찾지 못함");

        // ---------- 3. Books_19 ----------
        GameObject book = FindInScene("Books_19");
        if (book != null)
        {
            BookInteract bi = GetOrAdd<BookInteract>(book);
            bi.code = Code;
            MakeInteractable(book, layer);
            log.Add("Books_19: BookInteract 설정 (숫자 " + Code + ")");
        }
        else log.Add("[주의] Books_19 오브젝트를 찾지 못함");

        // ---------- 4. 노트북 ----------
        SetupLaptop(font, layer);

        // ---------- 5. Office_Files + 최종 열쇠 ----------
        SetupOfficeFiles(layer);

        // ---------- 6. UI (NoteUI / QuizUI) ----------
        SetupUI(font);

        // ---------- 7. 플레이어 레이어 마스크 확인 ----------
        PlayerInteraction pi = Object.FindFirstObjectByType<PlayerInteraction>();
        if (pi != null && layer >= 0 && (pi.interactLayer.value & (1 << layer)) == 0)
        {
            pi.interactLayer |= (1 << layer);
            EditorUtility.SetDirty(pi);
            log.Add("PlayerInteraction의 Interact Layer에 Interactable 추가");
        }

        // 저장
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        string result = string.Join("\n", log.ToArray());
        Debug.Log("[퍼즐 자동 설치]\n" + result);
        EditorUtility.DisplayDialog("퍼즐 자동 설치 완료", result +
            "\n\n※ 노트북의 ScreenText 위치는 씬 뷰에서 화면 위에 오도록 직접 조정해 주세요.", "확인");
    }

    // ============================================================
    // 노트북: Laptop_off에 직접 LaptopInteract를 붙임
    // (E키 → 비밀번호 입력 텍스트 표시, 모델 전환 없음)
    // ============================================================
    static void SetupLaptop(TMP_FontAsset font, int layer)
    {
        GameObject off = FindInScene("Laptop_off");
        GameObject on = FindInScene("Laptop_on");

        if (off == null)
        {
            log.Add("[주의] Laptop_off 오브젝트를 찾지 못함");
            return;
        }

        // 이전 방식(Laptop 부모에 컴포넌트 + 큰 트리거 콜라이더 + Laptop_on 화면 텍스트)의 흔적 정리
        foreach (LaptopInteract old in Object.FindObjectsByType<LaptopInteract>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (old.gameObject == off) continue;

            BoxCollider oldBox = old.GetComponent<BoxCollider>();
            if (oldBox != null && oldBox.isTrigger)
            {
                Object.DestroyImmediate(oldBox);
            }
            if (old.screenText != null)
            {
                Object.DestroyImmediate(old.screenText.gameObject);
            }
            Object.DestroyImmediate(old);
            log.Add("이전 노트북 설정 제거");
        }

        // Laptop_on 모델은 더 이상 사용하지 않음
        if (on != null && on.activeSelf)
        {
            on.SetActive(false);
            log.Add("Laptop_on 비활성화 (더 이상 사용 안 함)");
        }
        if (!off.activeSelf)
        {
            off.SetActive(true);
        }

        LaptopInteract li = GetOrAdd<LaptopInteract>(off);
        li.correctCode = Code;
        li.successText = "Studio 29";

        // 노트북 화면 텍스트 (3D TextMeshPro) - 평소엔 스크립트가 숨기고, E키를 누르면 표시
        if (li.screenText == null)
        {
            Transform existing = off.transform.Find("ScreenText");
            TextMeshPro tmp = existing != null ? existing.GetComponent<TextMeshPro>() : null;
            if (tmp == null)
            {
                GameObject textGO = new GameObject("ScreenText", typeof(RectTransform));
                textGO.transform.SetParent(off.transform, false);
                tmp = textGO.AddComponent<TextMeshPro>();
                tmp.font = font;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 0.01f;
                tmp.fontSizeMax = 10f;
                tmp.color = new Color(0.4f, 1f, 0.6f); // 화면 느낌의 연두색
                tmp.text = "PASSWORD\n_ _ _ _";

                // 노트북 크기에 맞춰 대략적인 위치/크기 설정 (이후 씬 뷰에서 미세조정)
                Bounds b = GetBounds(off);
                RectTransform rt = textGO.GetComponent<RectTransform>();
                float w = Mathf.Max(b.size.x, b.size.z);
                rt.sizeDelta = new Vector2(w * 0.7f, b.size.y * 0.6f);
                textGO.transform.position = b.center;
                textGO.transform.rotation = off.transform.rotation;
            }
            li.screenText = tmp;
            log.Add("Laptop_off 밑에 ScreenText(3D 텍스트) 생성/연결");
        }

        MakeInteractable(off, layer);
        EditorUtility.SetDirty(li);
        log.Add("노트북: E키 → 비밀번호 입력 (암호 " + Code + " → Studio 29)");
    }

    // ============================================================
    // Office_Files: 퀴즈 + 최종 열쇠 생성
    // ============================================================
    static void SetupOfficeFiles(int layer)
    {
        GameObject files = FindInScene("Office_Files");
        if (files == null)
        {
            log.Add("[주의] Office_Files 오브젝트를 찾지 못함");
            return;
        }

        OfficeFilesInteract ofi = GetOrAdd<OfficeFilesInteract>(files);
        MakeInteractable(files, layer);

        // 최종 열쇠 아이템 에셋 생성 (이미 있으면 재사용)
        Item keyItem = AssetDatabase.LoadAssetAtPath<Item>(FinalKeyItemPath);
        if (keyItem == null)
        {
            keyItem = ScriptableObject.CreateInstance<Item>();
            keyItem.itemName = FinalKeyName;
            keyItem.icon = AssetDatabase.LoadAssetAtPath<Sprite>(KeyIconPath);
            keyItem.isKey = true;
            AssetDatabase.CreateAsset(keyItem, FinalKeyItemPath);
            log.Add("최종 열쇠 아이템 에셋 생성: " + FinalKeyItemPath);
        }

        // 씬에 최종 열쇠 오브젝트 생성 (서류 바로 밑에 숨김)
        if (ofi.finalKey == null)
        {
            GameObject keyGO = FindInScene("FinalKey");
            if (keyGO == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(KeyPrefabPath);
                if (prefab != null)
                {
                    keyGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                }
                else
                {
                    keyGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    keyGO.transform.localScale = Vector3.one * 0.05f;
                }
                keyGO.name = "FinalKey";

                // 서류와 같은 부모로 (서랍이 열릴 때 같이 움직이도록)
                keyGO.transform.SetParent(files.transform.parent, true);
                Bounds fb = GetBounds(files);
                keyGO.transform.position = fb.center + Vector3.down * Mathf.Max(fb.extents.y * 0.5f, 0.01f);
            }

            ItemPickup pickup = GetOrAdd<ItemPickup>(keyGO);
            pickup.itemData = keyItem;
            MakeInteractable(keyGO, layer);

            ofi.finalKey = keyGO;
            log.Add("FinalKey 오브젝트 생성 후 Office_Files 밑에 배치 (시작 시 자동 숨김)");
        }

        EditorUtility.SetDirty(ofi);
        log.Add("Office_Files: OfficeFilesInteract 설정 완료 (답: Studio 29)");
    }

    // ============================================================
    // Canvas 밑에 NoteUI / QuizUI 패널 생성
    // ============================================================
    static void SetupUI(TMP_FontAsset font)
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            log.Add("[주의] Canvas를 찾지 못해 UI를 만들지 못함");
            return;
        }
        Transform cv = canvas.transform;

        // ----- NoteUI -----
        NoteUI noteUI = GetOrAdd<NoteUI>(canvas.gameObject);
        if (noteUI.panel == null)
        {
            GameObject panel = CreatePanel(cv, "NotePanel", new Color(0f, 0f, 0f, 0.85f), true);

            TextMeshProUGUI text = CreateText(panel.transform, "NoteText", font, 40, "");
            Stretch(text.rectTransform, 100, 100);

            TextMeshProUGUI hint = CreateText(panel.transform, "CloseHint", font, 22, "[E] 닫기");
            hint.color = new Color(1f, 1f, 1f, 0.5f);
            RectTransform hrt = hint.rectTransform;
            hrt.anchorMin = new Vector2(0.5f, 0f);
            hrt.anchorMax = new Vector2(0.5f, 0f);
            hrt.anchoredPosition = new Vector2(0, 50);
            hrt.sizeDelta = new Vector2(400, 40);

            noteUI.panel = panel;
            noteUI.noteText = text;
            panel.SetActive(false);
            EditorUtility.SetDirty(noteUI);
            log.Add("NotePanel(메모 UI) 생성 및 NoteUI 연결");
        }

        // ----- QuizUI -----
        QuizUI quizUI = GetOrAdd<QuizUI>(canvas.gameObject);
        if (quizUI.panel == null)
        {
            GameObject panel = CreatePanel(cv, "QuizPanel", new Color(0.07f, 0.07f, 0.1f, 0.95f), false);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.sizeDelta = new Vector2(720, 420);

            TextMeshProUGUI question = CreateText(panel.transform, "QuestionText", font, 30, "문제");
            RectTransform qrt = question.rectTransform;
            qrt.anchorMin = new Vector2(0, 0.45f);
            qrt.anchorMax = new Vector2(1, 1);
            qrt.offsetMin = new Vector2(30, 0);
            qrt.offsetMax = new Vector2(-30, -25);

            // TMP 기본 InputField 생성
            TMP_DefaultControls.Resources res = new TMP_DefaultControls.Resources();
            res.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            res.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
            GameObject inputGO = TMP_DefaultControls.CreateInputField(res);
            inputGO.name = "AnswerInput";
            inputGO.transform.SetParent(panel.transform, false);
            RectTransform irt = inputGO.GetComponent<RectTransform>();
            irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
            irt.sizeDelta = new Vector2(440, 60);
            irt.anchoredPosition = new Vector2(0, -60);

            TMP_InputField input = inputGO.GetComponent<TMP_InputField>();
            input.fontAsset = font;
            if (input.textComponent != null)
            {
                input.textComponent.font = font;
                input.textComponent.fontSize = 28;
            }
            TextMeshProUGUI ph = input.placeholder as TextMeshProUGUI;
            if (ph != null)
            {
                ph.font = font;
                ph.fontSize = 28;
                ph.text = "답을 입력하고 Enter...";
            }

            // 확인 / 닫기 버튼
            Button submit = CreateButton(panel.transform, "SubmitButton", font, "확인",
                new Vector2(-90, -150), res.standard);
            UnityEventTools.AddPersistentListener(submit.onClick, quizUI.Submit);

            Button close = CreateButton(panel.transform, "CloseButton", font, "닫기",
                new Vector2(90, -150), res.standard);
            UnityEventTools.AddPersistentListener(close.onClick, quizUI.Close);

            quizUI.panel = panel;
            quizUI.questionText = question;
            quizUI.answerInput = input;
            panel.SetActive(false);
            EditorUtility.SetDirty(quizUI);
            log.Add("QuizPanel(퀴즈 UI) 생성 및 QuizUI 연결");
        }
    }

    // ============================================================
    // 헬퍼
    // ============================================================

    // 비활성 오브젝트 포함, 이름으로 씬에서 검색 (대소문자 무시)
    static GameObject FindInScene(string name)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(t.name, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return t.gameObject;
                }
            }
        }
        return null;
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        EditorUtility.SetDirty(go);
        return c;
    }

    // 레이어 + 콜라이더 보장 (시선 감지가 되도록)
    static void MakeInteractable(GameObject go, int layer)
    {
        SetLayerRecursive(go, layer);

        if (go.GetComponentInChildren<Collider>(true) == null)
        {
            BoxCollider box = go.AddComponent<BoxCollider>();
            Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
            if (rs.Length > 0)
            {
                Bounds b = rs[0].bounds;
                foreach (Renderer r in rs) b.Encapsulate(r.bounds);
                box.center = go.transform.InverseTransformPoint(b.center);
                Vector3 ls = go.transform.lossyScale;
                box.size = new Vector3(
                    b.size.x / Mathf.Max(Mathf.Abs(ls.x), 0.0001f),
                    b.size.y / Mathf.Max(Mathf.Abs(ls.y), 0.0001f),
                    b.size.z / Mathf.Max(Mathf.Abs(ls.z), 0.0001f));
            }
        }
    }

    static void SetLayerRecursive(GameObject go, int layer)
    {
        if (layer < 0) return;
        go.layer = layer;
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = layer;
        }
    }

    static Bounds GetBounds(GameObject go)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one * 0.3f);
        Bounds b = rs[0].bounds;
        foreach (Renderer r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    static GameObject CreatePanel(Transform parent, string name, Color color, bool fullScreen)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (fullScreen)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }
        return panel;
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, TMP_FontAsset font, float size, string content)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.text = content;
        return text;
    }

    static void Stretch(RectTransform rt, float marginX, float marginY)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(marginX, marginY);
        rt.offsetMax = new Vector2(-marginX, -marginY);
    }

    static Button CreateButton(Transform parent, string name, TMP_FontAsset font, string label,
        Vector2 pos, Sprite sprite)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.25f, 0.3f, 0.4f, 1f);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(150, 56);
        rt.anchoredPosition = pos;

        TextMeshProUGUI text = CreateText(go.transform, "Text", font, 26, label);
        Stretch(text.rectTransform, 0, 0);

        return go.GetComponent<Button>();
    }
}
