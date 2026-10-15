namespace LabApi.Enum;
public enum ContainerFilterOperator
{
    eq,         //equal  
    neq,        //not equal
    gt,         //greater than
    gte,        //greater than or equal
    lt,         //less than
    lte,        //less than or equal
    contains,   //string contains
    startswith, //string starts with
    endswith    //string ends with
}