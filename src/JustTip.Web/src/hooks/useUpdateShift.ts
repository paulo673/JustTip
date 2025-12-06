import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api'
import type { ShiftDto } from '../types/roster'

export interface UpdateShiftRequest {
  employeeId: number
  date: string
  startTime: string
  endTime: string
}

interface UpdateShiftParams {
  id: number
  request: UpdateShiftRequest
}

async function updateShift({ id, request }: UpdateShiftParams): Promise<ShiftDto> {
  const response = await api.put<ShiftDto>(`/roster/shifts/${id}`, request)
  return response.data
}

export function useUpdateShift() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: updateShift,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] })
    },
  })
}
