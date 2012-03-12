using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace LazyAccountant
{
    public class Employee
    {
        public string  m_id = string.Empty;
        public string  m_name = string.Empty;
        public string  m_email = string.Empty;

        public decimal m_internalSalary = 0.0M;
        public decimal m_externalSalary = 0.0M;

        public decimal m_socialInsuranceCut = 0.0M;//�籣�ۿ�
        public decimal m_houseFundCut = 0.0M;//ס��������ۿ�

        public decimal GetTotalSalary()
        {
            return m_internalSalary + m_externalSalary;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("����:   {0}\n", m_id);
            sb.AppendFormat("����:   {0}\n", m_name);
            sb.AppendFormat("����:   {0}\n", m_email);
            sb.AppendFormat("���ڹ���:   {0:0.00}\n", m_internalSalary);
            sb.AppendFormat("���⹤��:   {0:0.00}\n", m_externalSalary);
            sb.AppendFormat("�籣�ۿ�:   {0:0.00}\n", m_socialInsuranceCut);
            sb.AppendFormat("������ۿ�: {0:0.00}\n", m_houseFundCut);
            return sb.ToString();
        }
    }

    //���㹤����Ҫ�Ĺؼ�����
    public class CalcArg
    {
        public string m_employeeId = string.Empty;//Ա��

        public string m_month = string.Empty;//e.g. 201005

        public decimal m_allowance = 0.0M;//����, �����ȶ�������

        public int m_late = 0;//�ٵ�����

        public float m_absent = 0.0F;//ȱ������

        public decimal m_previousTaxCut = 0.0M;//�ϸ��¸�˰

        public decimal m_otherCut = 0.0M;//�����۳����

        public override string ToString()
        {
            return string.Format("{0},{1:0.00},{2},{3:0.00},{4:0.00},{5:0.00}",
                m_employeeId,//0
                m_allowance, //2
                m_late, //3, int
                m_absent, //4, float
                m_previousTaxCut, //5
                m_otherCut);//6
        }

        public void FromString(string str)
        {
            string[] infos = str.Split(',');
            int index = 0;
            m_employeeId = infos[index];
            decimal.TryParse(infos[++index], out m_allowance);
            int.TryParse(infos[++index], out m_late);
            float.TryParse(infos[++index], out m_absent);
            decimal.TryParse(infos[++index], out m_previousTaxCut);
            decimal.TryParse(infos[++index], out m_otherCut);
        }
    }

    //��������
    public class Salary
    {
        //������Ҫ����һ��Ա����Ϣ, ����������id, ��ΪԱ����Ϣ�ᷢ���仯, �����Ǳ����н��ʱ��Ա������
        public Employee m_employee = new Employee();

        public CalcArg m_args = new CalcArg();

        public decimal m_lateCut;//�ٵ��ۿ�

        public decimal m_absentCut;//ȱ�ڿۿ�

        public decimal m_incomeToTax;//Ӧ��˰����

        public decimal m_taxToCut;//��˰, ���ݱ��������������, �¸����ٿ۳�

        public decimal m_internalIncome;

        public decimal m_externalIncome;

        public decimal m_totalIncome;

        //��ŵ��н����:
        //�۵��籣�͹�����
        //����˾�涨���ӺͿ۳�, ���罱��, �ٵ�, ȱ��, �Լ�����, ���𻵹�����⳥    
        //�����˰(��������), ������µĸ�˰�����¸����ٿ۳�
        //�۵��ϸ��¸�˰, ע���������ȼ����˰, Ȼ���ٿ۳��ϸ��µĸ�˰, �е㲻����
        //�õ�ʵ�����
        public void Calc(int workdayCount)
        {
            //����ٵ��ۿ�
            m_lateCut = m_args.m_late * DataCenter.Instance.LateCutUnit;

            //ȱ�ڿۿ�
            m_absentCut = m_employee.GetTotalSalary() * (decimal)m_args.m_absent / workdayCount;

            //����Ӧ��˰����
            m_incomeToTax = m_employee.m_internalSalary - m_employee.m_socialInsuranceCut - m_employee.m_houseFundCut + m_args.m_allowance - m_lateCut - m_absentCut - m_args.m_otherCut;

            m_taxToCut = IndividualIncomeTax.GetTax(DataCenter.Instance.IndividualIncomeTaxStart, m_incomeToTax);//�����˰, ������²���

            //�ܼ�����
            m_internalIncome = m_incomeToTax - m_args.m_previousTaxCut;//���ϸ��µ�˰

            m_externalIncome = m_employee.m_externalSalary;

            m_totalIncome = m_internalIncome + m_externalIncome;
        }

        public string ToExcelLine()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("{0}\t{1}\t{2:0.00}\t{3:0.00}\t{4:0.00}\t{5:0.00}\t{6:0.00}\t{7:0.00}\t{8:0.00}\t{9:0.00}\t{10:0.00}\t{11:0.00}",
                m_employee.m_id, m_employee.m_name,
                    m_employee.m_internalSalary, m_employee.m_externalSalary,
                    m_lateCut, m_absentCut, m_args.m_allowance,
                    m_employee.GetTotalSalary() - m_lateCut - m_absentCut,
                    m_employee.m_socialInsuranceCut, m_employee.m_houseFundCut, m_args.m_previousTaxCut,
                    m_totalIncome);
            return b.ToString();
        }

        public override string ToString()
        {
            if (m_totalIncome <= 0.0M)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format("Ա������:       {0}", m_employee.m_name));

            if (m_args.m_allowance > 0.0M)
            {
                sb.AppendLine(String.Format("������:       {0:0.00}", m_args.m_allowance));
            }

            if (m_lateCut > 0.0M)
            {
                sb.AppendLine(String.Format("�ٵ��ۿ�:       {0:0.00}", m_lateCut));
            }

            if (m_absentCut > 0.0M)
            {
                sb.AppendLine(String.Format("ȱ�ڿۿ�:       {0:0.00}", m_absentCut));
            }

            if (m_employee.m_socialInsuranceCut > 0.0M)
            {
                sb.AppendLine(String.Format("�籣�ۿ�:       {0:0.00}", m_employee.m_socialInsuranceCut));
            }

            if (m_employee.m_houseFundCut > 0.0M)
            {
                sb.AppendLine(String.Format("������ۿ�:     {0:0.00}", m_employee.m_houseFundCut));
            }

            if (m_args.m_otherCut > 0.0M)
            {
                sb.AppendLine(String.Format("�����ۿ�:       {0:0.00}", m_args.m_otherCut));
            }

            if (m_incomeToTax > 0.0M)
            {
                sb.AppendLine(String.Format("Ӧ��˰����:     {0:0.00}", m_incomeToTax));
            }

            //if (m_taxToCut > 0)
            //{
            //    sb.AppendLine(String.Format("����Ӧ�ɸ�˰:   {0:0.00}(�¸����ٽ�)", m_taxToCut));
            //}

            if (m_args.m_previousTaxCut > 0.0M)
            {
                sb.AppendLine(String.Format("���¸�˰�ۿ�:   {0:0.00}(�ϸ���δ��, ���¿۳�)", m_args.m_previousTaxCut));
            }

            if (m_externalIncome > 0.0M)
            {
                sb.AppendLine(String.Format("��������:       {0:0.00}", m_internalIncome));

                sb.AppendLine(String.Format("��������:       {0:0.00}", m_externalIncome));
            }

            sb.AppendLine(String.Format("ȫ������:       {0:0.00}", m_totalIncome));

            return sb.ToString();
        }

        //��ȡһ��ʱ�䷶Χ���ж��ٸ�������, ������to��һ��, ����, ����Ϊ2010.04.01 - 2010.05.01, �������4�·ݵ�Ӧ��������
        public static int GetWorkdayCount(DateTime from, DateTime to)
        {
            DateTime date = from;
            int workdayCount = 0;
            while (date < to)
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    ++workdayCount;
                }
                date = date.AddDays(1);
            }
            return workdayCount;
        }

        //��ȡĳ���ĳ�����ж��ٸ�������
        public static int GetWorkdayCount(int year, int month)
        {
            DateTime from = new DateTime(year, month, 1);
            DateTime to = from.AddMonths(1);
            return GetWorkdayCount(from, to);
        }

        public static int GetWorkdayCount(DateTime month)
        {
            DateTime from = new DateTime(month.Year, month.Month, 1);
            DateTime to = from.AddMonths(1);
            return GetWorkdayCount(from, to);
        }
    }
}
