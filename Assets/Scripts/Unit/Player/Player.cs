using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{
    // start()에서 생성자를 통해 객체의 초기값을 초기화시켜주면 GameManager에서 비정상적인 순서로 동작하여 GlobalEffect가 적용되지 않는 문제가 있었습니다.
    // GameManager에서 GlobalEffect State가 실행된 이후 Start가 호출되어 정상동작을 하지 않았습니다.(Race Condition)
    // async, IEnumerator 등으로 해주려 했지만, 기존 코드 논리에 영향을 주게 될 가능성이 있어서 다음과 같이 변경해주었습니다.
    // 이를 해결시켜주기 위해 생성자가 아닌 Initialize 함수를 통해 초기화 시켜주는 방식으로 변경시켰습니다.
    // 해당 함수의 인자는 기존 생성자와 동일하며, Unit 스크립트에 선언되어 있습니다.
    // 만약 Unit 객체를 생성해주었을 경우(Instantiate 등) 가급적이면 바로 다음에 Initialize 함수를 호출해주어 초기화 시켜주는 방식을 권합니다.
    // Update is called once per frame
    public override void Init()
    {
        Unit_Id = 1;
        Unit_Name = "Unit_Tank";
        maxMovingCost = 5;
        type = Unit_Type.Armored;
        maxHp = 5;
        attackPower = 3;
        armor = 2;
        attackRange = 2;
        maxAttackCnt = 1;
        visionCost = 7;
        Current_HP = maxHp;
    }

    public override void Attack(Unit targetUnit)
    {
        if (targetUnit.GetComponent<Enemy>() == null)
        {
            base.Attack(targetUnit);
        }

        else
        {
            targetUnit.OnDamage(attackPower);
            base.Attack(targetUnit);
        }
    }

    public override void moveTo(GameObject targetGrid){

        // Script Extract
        Grid targetGridScript = targetGrid.GetComponent<Grid>();
        Grid currentGridScript = transform.parent.GetComponent<Grid>();

        movingCost = targetGridScript.movementCostAfterArrive; // 현재 발판에서 타겟 발판까지 이동한 후 코스트

        // 이동한 유닛의 부모를 타겟발판으로 설정하고 위치 재설정
        // 이동 애니메이션 추가 필요(스무스하게 움직이도록)
        transform.SetParent(targetGrid.transform);
        this.transform.position = new Vector3(targetGrid.transform.position.x, targetGrid.transform.position.y, this.transform.position.z);
        this.Current_Grid = targetGrid.GetComponent<Grid>();
        this.CalculateVision();
    }
}
