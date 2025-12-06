import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { format, startOfWeek } from 'date-fns'
import { api } from '../lib/api'

export interface DailyTipInput {
  date: string
  amount: number
}

export interface EmployeePayout {
  employeeName: string
  totalHours: number
  payoutAmount: number
}

export interface PayoutResponse {
  totalWeeklyTips: number
  totalWeeklyHours: number
  employeePayouts: EmployeePayout[]
}

export interface DailyTipResponse {
  date: string
  amount: number
}

async function fetchPayout(weekStart: Date): Promise<PayoutResponse> {
  const formattedDate = format(weekStart, 'yyyy-MM-dd')
  const response = await api.get<PayoutResponse>(`/tips/payout?weekStart=${formattedDate}`)
  return response.data
}

async function fetchDailyTips(weekStart: Date): Promise<DailyTipResponse[]> {
  const formattedDate = format(weekStart, 'yyyy-MM-dd')
  const response = await api.get<DailyTipResponse[]>(`/tips/daily?weekStart=${formattedDate}`)
  return response.data
}

async function saveDailyTips(tips: DailyTipInput[]): Promise<void> {
  await api.post('/tips/daily', tips)
}

export function useTipsPayout(selectedDate: Date) {
  const weekStart = startOfWeek(selectedDate, { weekStartsOn: 1 })

  return useQuery({
    queryKey: ['tips-payout', format(weekStart, 'yyyy-MM-dd')],
    queryFn: () => fetchPayout(weekStart),
  })
}

export function useDailyTips(selectedDate: Date) {
  const weekStart = startOfWeek(selectedDate, { weekStartsOn: 1 })

  return useQuery({
    queryKey: ['daily-tips', format(weekStart, 'yyyy-MM-dd')],
    queryFn: () => fetchDailyTips(weekStart),
  })
}

export function useSaveTips() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: saveDailyTips,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tips-payout'] })
      queryClient.invalidateQueries({ queryKey: ['daily-tips'] })
    },
  })
}
