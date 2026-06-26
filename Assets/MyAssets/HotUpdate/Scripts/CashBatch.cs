using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CashBatch : MonoBehaviour
{
    public Transform[] money;
    public float val, jumpPower, duration, durationRot;

    void Start()
    {
        Vector3 vec = new Vector3(money[0].transform.localPosition.x - val, -0.462f, money[0].transform.localPosition.z + val);

        Vector3 vec1 = new Vector3(money[1].transform.localPosition.x + val, -0.462f, money[1].transform.localPosition.z + val);

        Vector3 vec2 = new Vector3(money[2].transform.localPosition.x - val, -0.462f, money[2].transform.localPosition.z - val);

        Vector3 vec3 = new Vector3(money[3].transform.localPosition.x + val, -0.462f, money[3].transform.localPosition.z - val);

        JumpAndRotate(0, vec, new Vector3(360, 360, 0));
        JumpAndRotate(1, vec1, new Vector3(0, 360, 360));
        JumpAndRotate(2, vec2, new Vector3(360, 0, 360));
        JumpAndRotate(3, vec3, new Vector3(360, 360, 360));
    }

    private Tweener[] _rotateTweens;

    private void Awake()
    {
        _rotateTweens = new Tweener[money.Length];
    }

    private void JumpAndRotate(int index, Vector3 vec, Vector3 rotVec)
    {
        money[index].DOLocalJump(vec, jumpPower, 1, duration).OnComplete(delegate ()
        {
            if (money[index] != null)
                money[index].GetComponent<BoxCollider>().enabled = true;
        });

        _rotateTweens[index] = money[index].DORotate(rotVec, durationRot * 0.5f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _rotateTweens.Length; i++)
        {
            if (_rotateTweens[i] != null && _rotateTweens[i].IsActive())
                _rotateTweens[i].Kill();
        }
    }

    private void Update()
    {
        if (transform.childCount == 0)
            Destroy(gameObject);
    }
}
