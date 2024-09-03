def esBisiesto(anyo):
    if ((anyo % 4 == 0) and (anyo % 100 != 0)) or (anyo % 400 == 0):
        return True
    return False

def diaAnyo(dd,m,YYYY):
    diasMes = [31,28,31,30,31,30,31,31,30,31,30,31]
    if esBisiesto(2020) == True:
        diasMes[1] += 1
    dia = 0
    for i in range(m-1):
        dia += diasMes[i]
    dia += dd
    return dia