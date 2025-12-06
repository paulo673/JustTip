import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api'
import type { ShiftDto } from '../types/roster'

export interface CreateShiftRequest {
  employeeId: number
  date: string
  startTime: string
  endTime: string
}

async function createShift(request: CreateShiftRequest): Promise<ShiftDto> {
  const response = await api.post<ShiftDto>('/roster/shifts', request)
  return response.data
}

export function useCreateShift() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createShift,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] })
    },
  })
}
